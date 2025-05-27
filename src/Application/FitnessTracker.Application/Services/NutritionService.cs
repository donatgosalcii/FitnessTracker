using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Nutrition;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities.Nutrition;
using FitnessTracker.Domain.Enums.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FitnessTracker.Application.Services
{
    public class NutritionService : INutritionService
    {
        private readonly IFoodItemRepository _foodItemRepository;
        private readonly ILoggedFoodItemRepository _loggedFoodItemRepository;
        private readonly IUserNutritionGoalRepository _userNutritionGoalRepository;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<NutritionService> _logger;

        public NutritionService(
            IFoodItemRepository foodItemRepository,
            ILoggedFoodItemRepository loggedFoodItemRepository,
            IUserNutritionGoalRepository userNutritionGoalRepository,
            IApplicationDbContext context,
            ILogger<NutritionService> logger)
        {
            _foodItemRepository = foodItemRepository ?? throw new ArgumentNullException(nameof(foodItemRepository));
            _loggedFoodItemRepository = loggedFoodItemRepository ?? throw new ArgumentNullException(nameof(loggedFoodItemRepository));
            _userNutritionGoalRepository = userNutritionGoalRepository ?? throw new ArgumentNullException(nameof(userNutritionGoalRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<FoodItemDto>> CreateFoodItemAsync(CreateFoodItemDto dto, string userId)
        {
            _logger.LogInformation("Attempting to create food item '{FoodName}' for user {UserId}", dto?.Name, userId);
            if (dto == null) { return Result<FoodItemDto>.ValidationFailed("Input data is null.", "CreateFoodItem.NullDto"); }
            if (string.IsNullOrWhiteSpace(userId)) { return Result<FoodItemDto>.ValidationFailed("User ID is null or empty.", "CreateFoodItem.NullUserId"); }
            if (string.IsNullOrWhiteSpace(dto.Name)) { return Result<FoodItemDto>.ValidationFailed("Food item name is required.", "CreateFoodItem.NameRequired"); }

            var foodItem = new FoodItem();
            MapFromCreateDtoToFoodItem(dto, foodItem, userId);

            var addedFoodItem = await _foodItemRepository.AddAsync(foodItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Food item '{FoodName}' created successfully with ID {FoodItemId} for user {UserId}", addedFoodItem.Name, addedFoodItem.Id, userId);

            return Result<FoodItemDto>.Success(MapToFoodItemDto(addedFoodItem));
        }

        public async Task<Result<FoodItemDto>> UpdateFoodItemAsync(int foodItemId, UpdateFoodItemDto dto, string userId)
        {
            _logger.LogInformation("Attempting to update food item ID {FoodItemId} for user {UserId}", foodItemId, userId);
            if (dto == null) { return Result<FoodItemDto>.ValidationFailed("Input data is null.", "UpdateFoodItem.NullDto"); }
            if (string.IsNullOrWhiteSpace(userId)) { return Result<FoodItemDto>.ValidationFailed("User ID is null or empty.", "UpdateFoodItem.NullUserId"); }

            var foodItem = await _foodItemRepository.GetByIdAsync(foodItemId);
            if (foodItem == null) { _logger.LogWarning("UpdateFoodItem: FoodItem with ID {FoodItemId} not found.", foodItemId); return Result<FoodItemDto>.NotFound("Food item not found.", "UpdateFoodItem.NotFound"); }
            if (foodItem.ApplicationUserId != userId && foodItem.ApplicationUserId != null) { _logger.LogWarning("UpdateFoodItem: User {UserId} unauthorized to update FoodItem {FoodItemId} owned by {OwnerId}", userId, foodItemId, foodItem.ApplicationUserId); return Result<FoodItemDto>.Unauthorized("You are not authorized to update this food item.", "UpdateFoodItem.Unauthorized"); }

            MapFromUpdateDtoToFoodItem(dto, foodItem);

            await _foodItemRepository.UpdateAsync(foodItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Food item ID {FoodItemId} updated successfully for user {UserId}", foodItemId, userId);

            return Result<FoodItemDto>.Success(MapToFoodItemDto(foodItem));
        }

        public async Task<Result> DeleteFoodItemAsync(int foodItemId, string userId)
        {
            _logger.LogInformation("Attempting to delete food item ID {FoodItemId} for user {UserId}", foodItemId, userId);
            if (string.IsNullOrWhiteSpace(userId)) { return Result.ValidationFailed("User ID is null or empty.", "DeleteFoodItem.NullUserId"); }

            var foodItem = await _foodItemRepository.GetByIdAsync(foodItemId);
            if (foodItem == null) { _logger.LogWarning("DeleteFoodItem: FoodItem with ID {FoodItemId} not found.", foodItemId); return Result.NotFound("Food item not found.", "DeleteFoodItem.NotFound"); }
            if (foodItem.ApplicationUserId != userId && foodItem.ApplicationUserId != null) { _logger.LogWarning("DeleteFoodItem: User {UserId} unauthorized to delete FoodItem {FoodItemId} owned by {OwnerId}", userId, foodItemId, foodItem.ApplicationUserId); return Result.Unauthorized("You are not authorized to delete this food item.", "DeleteFoodItem.Unauthorized"); }

            var loggedUses = await _loggedFoodItemRepository.FindAsync(lfi => lfi.FoodItemId == foodItemId);
            if (loggedUses.Any()) { _logger.LogWarning("DeleteFoodItem: FoodItem {FoodItemId} ('{FoodName}') is in use and cannot be deleted.", foodItemId, foodItem.Name); return Result.Conflict($"Cannot delete '{foodItem.Name}' as it has been logged. Consider archiving options.", "DeleteFoodItem.InUse"); }

            await _foodItemRepository.DeleteAsync(foodItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Food item ID {FoodItemId} deleted successfully for user {UserId}", foodItemId, userId);
            return Result.Success();
        }

        public async Task<Result<FoodItemDto?>> GetFoodItemByIdAsync(int foodItemId)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(foodItemId);
            if (foodItem == null) { return Result<FoodItemDto?>.Success(null); }
            return Result<FoodItemDto?>.Success(MapToFoodItemDto(foodItem));
        }

        public async Task<Result<IEnumerable<FoodItemDto>>> GetUserCustomFoodItemsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return Result<IEnumerable<FoodItemDto>>.ValidationFailed("User ID is null or empty.", "GetUserCustomFoodItems.NullUserId"); }
            var foodItems = await _foodItemRepository.GetByUserIdAsync(userId);
            return Result<IEnumerable<FoodItemDto>>.Success(foodItems.Select(MapToFoodItemDto).ToList());
        }

        public async Task<Result<IEnumerable<FoodItemDto>>> SearchFoodItemsAsync(string searchTerm, string? userId)
        {
            var foodItems = await _foodItemRepository.SearchByNameAsync(searchTerm, userId);
            return Result<IEnumerable<FoodItemDto>>.Success(foodItems.Select(MapToFoodItemDto).ToList());
        }

        public async Task<Result<LoggedFoodItemDto>> LogFoodAsync(LogFoodItemRequestDto dto, string userId)
        {
            _logger.LogInformation("Attempting to log food item ID {FoodItemId} for user {UserId}", dto?.FoodItemId, userId);
            if (dto == null) { return Result<LoggedFoodItemDto>.ValidationFailed("Input data is null.", "LogFood.NullDto"); }
            if (string.IsNullOrWhiteSpace(userId)) { return Result<LoggedFoodItemDto>.ValidationFailed("User ID is null or empty.", "LogFood.NullUserId"); }

            var foodItem = await _foodItemRepository.GetByIdAsync(dto.FoodItemId);
            if (foodItem == null) { _logger.LogWarning("LogFood: FoodItem with ID {FoodItemId} not found.", dto.FoodItemId); return Result<LoggedFoodItemDto>.NotFound("The specified food item does not exist.", "LogFood.FoodItemNotFound"); }
            if (foodItem.ApplicationUserId != null && foodItem.ApplicationUserId != userId) { _logger.LogWarning("LogFood: User {UserId} unauthorized to log FoodItem {FoodItemId}", userId, dto.FoodItemId); return Result<LoggedFoodItemDto>.Unauthorized("You do not have access to this food item.", "LogFood.FoodItemNotAccessible"); }

            var loggedFood = new LoggedFoodItem
            {
                ApplicationUserId = userId, FoodItemId = dto.FoodItemId, LoggedDate = dto.LoggedDate.Date,
                Timestamp = DateTime.UtcNow, MealContext = dto.MealContext, QuantityConsumed = dto.QuantityConsumed,
                CalculatedCalories = foodItem.CaloriesPerServing * dto.QuantityConsumed,
                CalculatedProtein = foodItem.ProteinPerServing * dto.QuantityConsumed,
                CalculatedCarbohydrates = foodItem.CarbohydratesPerServing * dto.QuantityConsumed,
                CalculatedFat = foodItem.FatPerServing * dto.QuantityConsumed,
            };
            var addedLog = await _loggedFoodItemRepository.AddAsync(loggedFood);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Food item ID {FoodItemId} logged successfully as LoggedFoodItemID {LoggedFoodId} for user {UserId}", dto.FoodItemId, addedLog.Id, userId);
            return Result<LoggedFoodItemDto>.Success(MapToLoggedFoodItemDto(addedLog, foodItem));
        }

        public async Task<Result> RemoveLoggedFoodAsync(int loggedFoodItemId, string userId)
        {
            _logger.LogInformation("Attempting to remove logged food item ID {LoggedFoodItemId} for user {UserId}", loggedFoodItemId, userId);
            if (string.IsNullOrWhiteSpace(userId)) { return Result.ValidationFailed("User ID is null or empty.", "RemoveLog.NullUserId"); }

            var loggedFood = await _loggedFoodItemRepository.GetByIdAsync(loggedFoodItemId);
            if (loggedFood == null) { _logger.LogWarning("RemoveLoggedFood: LoggedFoodItem with ID {LoggedFoodItemId} not found.", loggedFoodItemId); return Result.NotFound("Logged food entry not found.", "RemoveLog.NotFound"); }
            if (loggedFood.ApplicationUserId != userId) { _logger.LogWarning("RemoveLoggedFood: User {UserId} unauthorized to remove LoggedFoodItem {LoggedFoodItemId}", userId, loggedFoodItemId); return Result.Unauthorized("You are not authorized to remove this log entry.", "RemoveLog.Unauthorized"); }

            await _loggedFoodItemRepository.DeleteAsync(loggedFood);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Logged food item ID {LoggedFoodItemId} removed successfully for user {UserId}", loggedFoodItemId, userId);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<LoggedFoodItemDto>>> GetLoggedFoodsForDateAsync(string userId, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return Result<IEnumerable<LoggedFoodItemDto>>.ValidationFailed("User ID is null or empty.", "GetLoggedFoods.NullUserId"); }
            var loggedItems = await _loggedFoodItemRepository.GetByUserIdAndDateAsync(userId, date.Date);
            return Result<IEnumerable<LoggedFoodItemDto>>.Success(loggedItems.Select(lfi => MapToLoggedFoodItemDto(lfi, lfi.FoodItem)).ToList());
        }

        public async Task<Result<DailyNutritionSummaryDto>> GetDailyNutritionSummaryAsync(string userId, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return Result<DailyNutritionSummaryDto>.ValidationFailed("User ID is null or empty.", "GetSummary.NullUserId"); }

            var targetDate = date.Date;
            var loggedItems = await _loggedFoodItemRepository.GetByUserIdAndDateAsync(userId, targetDate);
            var userGoal = await _userNutritionGoalRepository.GetByUserIdAsync(userId); 

            var summary = new DailyNutritionSummaryDto(targetDate)
            {
                TotalCaloriesConsumed = loggedItems.Sum(li => li.CalculatedCalories),
                TotalProteinConsumed = loggedItems.Sum(li => li.CalculatedProtein),
                TotalCarbohydratesConsumed = loggedItems.Sum(li => li.CalculatedCarbohydrates),
                TotalFatConsumed = loggedItems.Sum(li => li.CalculatedFat),
                LoggedItems = loggedItems.Select(lfi => MapToLoggedFoodItemDto(lfi, lfi.FoodItem)).ToList(),
                Goal = userGoal != null ? MapToUserNutritionGoalDto(userGoal) : null
            };
            return Result<DailyNutritionSummaryDto>.Success(summary);
        }

        public async Task<Result<UserNutritionGoalDto>> SetUserNutritionGoalAsync(SetUserNutritionGoalDto dto, string userId)
        {
            _logger.LogInformation("SetUserNutritionGoalAsync for User: {UserId}, DTO TargetCalories: {TargetCalories}", userId, dto?.TargetCalories);

            if (dto == null) { return Result<UserNutritionGoalDto>.ValidationFailed("Input data is null.", "SetGoal.NullDto"); }
            if (string.IsNullOrWhiteSpace(userId)) { return Result<UserNutritionGoalDto>.ValidationFailed("User ID is null or empty.", "SetGoal.NullUserId"); }

            var existingGoalEntity = await _userNutritionGoalRepository.GetByUserIdAsync(userId);

            UserNutritionGoal goalToPersist;
            bool isNew = false;

            if (existingGoalEntity == null)
            {
                isNew = true;
                goalToPersist = new UserNutritionGoal();
                MapFromSetDtoToUserNutritionGoal(dto, goalToPersist, userId);
                await _userNutritionGoalRepository.AddAsync(goalToPersist);
            }
            else
            {
                goalToPersist = existingGoalEntity;
                MapFromSetDtoToUserNutritionGoal(dto, goalToPersist, userId);
                await _userNutritionGoalRepository.UpdateAsync(goalToPersist);
            }

            int recordsAffected = 0;
            try
            {
                recordsAffected = await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException while saving UserNutritionGoal for User ID {UserId}. Inner: {InnerMessage}", userId, dbEx.InnerException?.Message);
                return Result<UserNutritionGoalDto>.Failure(new Error("DbSaveError", dbEx.InnerException?.Message ?? dbEx.Message), ErrorType.Failure);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic exception while saving UserNutritionGoal for User ID {UserId}", userId);
                return Result<UserNutritionGoalDto>.Failure(new Error("GenericSaveError", ex.Message), ErrorType.Unexpected);
            }

            if (isNew && recordsAffected == 0)
            {
                _logger.LogError("SetUserNutritionGoal - SaveChangesAsync reported 0 records affected for an expected NEW UserNutritionGoal for User ID {UserId}.", userId);
                return Result<UserNutritionGoalDto>.Failure(new Error("DbNoInsert", "Database reported no new goal was inserted."), ErrorType.Failure);
            }
            
            var persistedGoalAfterSave = await _userNutritionGoalRepository.GetByUserIdAsync(userId);
            if (persistedGoalAfterSave == null)
            {
                _logger.LogError("SetUserNutritionGoal - CRITICAL: Goal for User {UserId} not found after SaveChangesAsync!", userId);
                return Result<UserNutritionGoalDto>.Failure(new Error("GoalLostAfterSave", "Goal could not be retrieved after saving."), ErrorType.Unexpected);
            }

            return Result<UserNutritionGoalDto>.Success(MapToUserNutritionGoalDto(persistedGoalAfterSave));
        }

        public async Task<Result<UserNutritionGoalDto?>> GetUserNutritionGoalAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return Result<UserNutritionGoalDto?>.ValidationFailed("User ID is null or empty.", "GetGoal.NullUserId"); }
            var goal = await _userNutritionGoalRepository.GetByUserIdAsync(userId);
            if (goal == null) { return Result<UserNutritionGoalDto?>.Success(null); }
            return Result<UserNutritionGoalDto?>.Success(MapToUserNutritionGoalDto(goal));
        }

        public async Task<Result<PagedResult<LoggedFoodItemDto>>> GetUserLoggedFoodHistoryAsync(
            string userId, int pageNumber, int pageSize,
            DateTime? startDate = null, DateTime? endDate = null, string? mealContextFilter = null)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return Result<PagedResult<LoggedFoodItemDto>>.ValidationFailed("User ID is required.", "LogHistory.UserIdRequired"); }
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            Expression<Func<LoggedFoodItem, bool>> predicate = lfi => lfi.ApplicationUserId == userId;
            if (startDate.HasValue) { var start = startDate.Value.Date; predicate = predicate.And(lfi => lfi.LoggedDate >= start); }
            if (endDate.HasValue) { var end = endDate.Value.Date.AddDays(1).AddTicks(-1); predicate = predicate.And(lfi => lfi.LoggedDate <= end); }
            if (!string.IsNullOrWhiteSpace(mealContextFilter)) { predicate = predicate.And(lfi => lfi.MealContext.ToLower().Contains(mealContextFilter.ToLower())); }

            var allMatchingItemsQuery = (await _loggedFoodItemRepository.FindAsync(predicate)).AsQueryable();
            int totalCount = await Task.Run(() => allMatchingItemsQuery.Count());
            var pagedItems = await Task.Run(() => allMatchingItemsQuery.OrderByDescending(lfi => lfi.LoggedDate).ThenByDescending(lfi => lfi.Timestamp).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
            
            var dtos = pagedItems.Select(lfi => MapToLoggedFoodItemDto(lfi, lfi.FoodItem)).ToList(); // Assumes FoodItem is eager loaded by FindAsync
            var pagedResultData = new PagedResult<LoggedFoodItemDto>(dtos, totalCount, pageNumber, pageSize);
            return Result<PagedResult<LoggedFoodItemDto>>.Success(pagedResultData);
        }

        private void MapFromCreateDtoToFoodItem(CreateFoodItemDto dto, FoodItem foodItem, string userId) 
        { 
            foodItem.Name = dto.Name; foodItem.Brand = dto.Brand; foodItem.ServingSizeValue = dto.ServingSizeValue; 
            foodItem.ServingUnit = dto.ServingUnit; foodItem.CaloriesPerServing = dto.CaloriesPerServing; 
            foodItem.ProteinPerServing = dto.ProteinPerServing; foodItem.CarbohydratesPerServing = dto.CarbohydratesPerServing; 
            foodItem.FatPerServing = dto.FatPerServing; foodItem.FiberPerServing = dto.FiberPerServing; 
            foodItem.SugarPerServing = dto.SugarPerServing; foodItem.SodiumPerServing = dto.SodiumPerServing; 
            foodItem.Barcode = dto.Barcode; foodItem.ApplicationUserId = userId;
        }
        private void MapFromUpdateDtoToFoodItem(UpdateFoodItemDto dto, FoodItem foodItem) 
        { 
            foodItem.Name = dto.Name; foodItem.Brand = dto.Brand; foodItem.ServingSizeValue = dto.ServingSizeValue; 
            foodItem.ServingUnit = dto.ServingUnit; foodItem.CaloriesPerServing = dto.CaloriesPerServing; 
            foodItem.ProteinPerServing = dto.ProteinPerServing; foodItem.CarbohydratesPerServing = dto.CarbohydratesPerServing; 
            foodItem.FatPerServing = dto.FatPerServing; foodItem.FiberPerServing = dto.FiberPerServing; 
            foodItem.SugarPerServing = dto.SugarPerServing; foodItem.SodiumPerServing = dto.SodiumPerServing; 
            foodItem.Barcode = dto.Barcode;
        }
        private FoodItemDto MapToFoodItemDto(FoodItem? foodItem) 
        { 
            if (foodItem == null) { _logger.LogWarning("MapToFoodItemDto received null FoodItem entity."); return new FoodItemDto { Name = "Error: FoodItem was null" }; } 
            return new FoodItemDto { 
                Id = foodItem.Id, Name = foodItem.Name, Brand = foodItem.Brand, 
                ServingSizeValue = foodItem.ServingSizeValue, ServingUnit = foodItem.ServingUnit, 
                CaloriesPerServing = foodItem.CaloriesPerServing, ProteinPerServing = foodItem.ProteinPerServing, 
                CarbohydratesPerServing = foodItem.CarbohydratesPerServing, FatPerServing = foodItem.FatPerServing, 
                FiberPerServing = foodItem.FiberPerServing, SugarPerServing = foodItem.SugarPerServing, 
                SodiumPerServing = foodItem.SodiumPerServing, ApplicationUserId = foodItem.ApplicationUserId, 
                Barcode = foodItem.Barcode 
            };
        }
        private LoggedFoodItemDto MapToLoggedFoodItemDto(LoggedFoodItem? loggedFood, FoodItem? foodItemDetails) 
        { 
            if (loggedFood == null) { _logger.LogWarning("MapToLoggedFoodItemDto received null LoggedFoodItem entity."); return new LoggedFoodItemDto { FoodItemName = "Error: LoggedFoodItem was null" }; } 
            return new LoggedFoodItemDto { 
                Id = loggedFood.Id, ApplicationUserId = loggedFood.ApplicationUserId, FoodItemId = loggedFood.FoodItemId, 
                FoodItemName = foodItemDetails?.Name ?? "Unknown/Deleted Food", FoodItemBrand = foodItemDetails?.Brand, 
                LoggedDate = loggedFood.LoggedDate, Timestamp = loggedFood.Timestamp, MealContext = loggedFood.MealContext, 
                QuantityConsumed = loggedFood.QuantityConsumed, CalculatedCalories = loggedFood.CalculatedCalories, 
                CalculatedProtein = loggedFood.CalculatedProtein, CalculatedCarbohydrates = loggedFood.CalculatedCarbohydrates, 
                CalculatedFat = loggedFood.CalculatedFat 
            };
        }
        private void MapFromSetDtoToUserNutritionGoal(SetUserNutritionGoalDto dto, UserNutritionGoal goal, string userId) 
        { 
            goal.ApplicationUserId = userId; goal.GoalType = dto.GoalType; 
            goal.TargetCalories = dto.TargetCalories; goal.TargetProteinGrams = dto.TargetProteinGrams; 
            goal.TargetCarbohydratesGrams = dto.TargetCarbohydratesGrams; goal.TargetFatGrams = dto.TargetFatGrams; 
            goal.LastUpdatedDate = DateTime.UtcNow;
        }
        private UserNutritionGoalDto MapToUserNutritionGoalDto(UserNutritionGoal? goal) 
        { 
            if (goal == null) { _logger.LogWarning("MapToUserNutritionGoalDto received null UserNutritionGoal entity."); return new UserNutritionGoalDto { GoalType = FitnessGoalType.NotSet }; } 
            return new UserNutritionGoalDto { 
                Id = goal.Id, ApplicationUserId = goal.ApplicationUserId, GoalType = goal.GoalType, 
                TargetCalories = goal.TargetCalories, TargetProteinGrams = goal.TargetProteinGrams, 
                TargetCarbohydratesGrams = goal.TargetCarbohydratesGrams, TargetFatGrams = goal.TargetFatGrams, 
                LastUpdatedDate = goal.LastUpdatedDate 
            };
        }
    }
}