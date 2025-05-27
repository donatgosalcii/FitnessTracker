using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Nutrition;

namespace FitnessTracker.Application.Interfaces
{
    public interface INutritionService
    {
        Task<Result<FoodItemDto>> CreateFoodItemAsync(CreateFoodItemDto createFoodItemDto, string userId);
        Task<Result<FoodItemDto>> UpdateFoodItemAsync(int foodItemId, UpdateFoodItemDto updateFoodItemDto, string userId);
        Task<Result> DeleteFoodItemAsync(int foodItemId, string userId);
        Task<Result<FoodItemDto?>> GetFoodItemByIdAsync(int foodItemId); 
        Task<Result<IEnumerable<FoodItemDto>>> GetUserCustomFoodItemsAsync(string userId);
        Task<Result<IEnumerable<FoodItemDto>>> SearchFoodItemsAsync(string searchTerm, string? userId); 

        Task<Result<LoggedFoodItemDto>> LogFoodAsync(LogFoodItemRequestDto logFoodDto, string userId);
        Task<Result> RemoveLoggedFoodAsync(int loggedFoodItemId, string userId);
        Task<Result<IEnumerable<LoggedFoodItemDto>>> GetLoggedFoodsForDateAsync(string userId, DateTime date);
        Task<Result<DailyNutritionSummaryDto>> GetDailyNutritionSummaryAsync(string userId, DateTime date);
        Task<Result<PagedResult<LoggedFoodItemDto>>> GetUserLoggedFoodHistoryAsync(string userId, int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null, string? mealContextFilter = null);
        Task<Result<UserNutritionGoalDto>> SetUserNutritionGoalAsync(SetUserNutritionGoalDto goalDto, string userId);
        Task<Result<UserNutritionGoalDto?>> GetUserNutritionGoalAsync(string userId);
    }
}