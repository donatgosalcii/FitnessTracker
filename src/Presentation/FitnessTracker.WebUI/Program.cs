using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.Services;
using FitnessTracker.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer; // For JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options; // For IOptions
using FitnessTracker.Infrastructure.Authentication;
using FitnessTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection; // For Data Protection
using System.IO; // For DirectoryInfo

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Settings for IOptions
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName)
);

// 2. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register IApplicationDbContext to resolve to the ApplicationDbContext instance
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// 3. Configure ASP.NET Core Identity (This ALREADY sets up the Identity.Application cookie scheme)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Configure Application Cookie settings (This CUSTOMIZES the cookie set up by AddIdentity)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 5. Configure Authentication Schemes (Prioritize Cookie for Default Auth/Challenge)
builder.Services.AddAuthentication(options =>
    {
        // Default scheme for interactive UI (Razor Pages) is Identity's cookie scheme
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;

        // *** Set Cookie as the default for AUTHENTICATE requests ***
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        // *** Set Cookie as the default for CHALLENGE requests ***
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    // Add ONLY the JWT Bearer authentication handler.
    // The cookie handler for IdentityConstants.ApplicationScheme is added by AddIdentity().
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme); // Configure this handler below using AddOptions

// Configure JwtBearerOptions separately (this is good)
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>, IWebHostEnvironment>((bearerOptions, jwtOptions, hostingEnv) =>
    {
        var settings = jwtOptions.Value;
        bearerOptions.SaveToken = true;
        bearerOptions.RequireHttpsMetadata = hostingEnv.IsProduction();
        bearerOptions.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Data Protection (Keep this if you added it)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/donatgosalci/FitnessTrackerKeys")) // <-- ADJUST PATH IF NEEDED
    .SetApplicationName("FitnessTrackerApp");


// 6. Register Application and Infrastructure Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IMuscleGroupRepository, MuscleGroupRepository>();
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();
// builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
// builder.Services.AddScoped<IExerciseService, ExerciseService>();
// builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
// builder.Services.AddScoped<IWorkoutService, WorkoutService>();

// 7. Configure HttpClient for calling your own API from Razor Pages
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7115/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// 8. Add MVC Controllers and Razor Pages services
builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    // Add other conventions as needed
});

// ==================================================
//                Build the App
// ==================================================
var app = builder.Build();

// ==================================================
//          Configure the HTTP request pipeline
// ==================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // IMPORTANT: Call before UseAuthorization
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Seed Roles (Keep your existing logic)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User" };
        logger.LogInformation("Starting role seeding...");
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded) { logger.LogInformation($"Role '{roleName}' created successfully."); }
                else { logger.LogError($"Error creating role '{roleName}'. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}"); }
            }
            else { logger.LogInformation($"Role '{roleName}' already exists."); }
        }
        logger.LogInformation("Finished role seeding.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database roles.");
    }
}

app.Run();