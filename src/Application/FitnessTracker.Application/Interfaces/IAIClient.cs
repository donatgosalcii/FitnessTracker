namespace FitnessTracker.Application.Interfaces
{
    public interface IAIClient
    {
        Task<string> GetResponseAsync(string message, string? systemPrompt = null);
        bool IsAvailable();
    }
} 