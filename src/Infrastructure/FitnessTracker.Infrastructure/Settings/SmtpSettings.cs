namespace FitnessTracker.Infrastructure.Settings
{
    public class SmtpSettings
    {
        public const string SectionName = "Email:Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty; 
        public string? SenderDisplayName { get; set; }
    }
}