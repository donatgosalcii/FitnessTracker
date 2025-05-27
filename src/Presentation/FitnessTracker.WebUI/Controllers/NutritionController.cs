using FitnessTracker.Application.DTOs.Nutrition;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitnessTracker.Application.Common; 

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NutritionController : ControllerBase
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager; 

        public NutritionController(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID cannot be found in claims. User might not be authenticated properly or claims are missing.");
            }
            return userId;
        }
        
        private IActionResult HandleErrorResult(Result failedResult)
        {
            var errorMessage = failedResult.Error?.Message ?? "An unknown error occurred processing your request.";
            var errorCode = failedResult.Error?.Code; 

            var errorResponsePayload = new { Message = errorMessage, Code = errorCode };

            return failedResult.ErrorType switch
            {
                ErrorType.NotFound => NotFound(errorResponsePayload),
                ErrorType.Validation => BadRequest(new { Message = errorMessage, Code = errorCode, Errors = ModelStateIfApplicable() }),
                ErrorType.Unauthorized => Unauthorized(errorResponsePayload),
                ErrorType.Conflict => Conflict(errorResponsePayload),
                ErrorType.Failure => BadRequest(errorResponsePayload), 
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server.", Code = "InternalServerError" }),
            };
        }
        private object? ModelStateIfApplicable()
        {
            if (ModelState.IsValid || !ModelState.Any()) return null;
            return ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        }

        [HttpPost("fooditems")] 
        public async Task<IActionResult> CreateFoodItem([FromBody] CreateFoodItemDto createFoodItemDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelStateIfApplicable());
            var userId = GetCurrentUserId();
            var result = await _nutritionService.CreateFoodItemAsync(createFoodItemDto, userId);
            if (result.IsSuccess && result.Value != null)
            {
                return CreatedAtAction(nameof(GetFoodItem), new { foodItemId = result.Value.Id }, result.Value);
            }
            return HandleErrorResult(result);
        }

        [HttpGet("fooditems/{foodItemId:int}")] 
        public async Task<IActionResult> GetFoodItem(int foodItemId)
        {
            var result = await _nutritionService.GetFoodItemByIdAsync(foodItemId);
            if (result.IsSuccess)
            {
                if (result.Value == null) return NotFound(new { Message = "Food item not found.", Code = "FoodItem.NotFound" });
                return Ok(result.Value);
            }
            return HandleErrorResult(result);
        }

        [HttpPut("fooditems/{foodItemId:int}")] 
        public async Task<IActionResult> UpdateFoodItem(int foodItemId, [FromBody] UpdateFoodItemDto updateFoodItemDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelStateIfApplicable());
            var userId = GetCurrentUserId();
            var result = await _nutritionService.UpdateFoodItemAsync(foodItemId, updateFoodItemDto, userId);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return HandleErrorResult(result);
        }

        [HttpDelete("fooditems/{foodItemId:int}")] 
        public async Task<IActionResult> DeleteFoodItem(int foodItemId)
        {
            var userId = GetCurrentUserId();
            var result = await _nutritionService.DeleteFoodItemAsync(foodItemId, userId);
            if (result.IsSuccess) return NoContent();
            return HandleErrorResult(result);
        }

        [HttpGet("fooditems/mycustom")] 
        public async Task<IActionResult> GetUserCustomFoodItems()
        {
            var userId = GetCurrentUserId();
            var result = await _nutritionService.GetUserCustomFoodItemsAsync(userId);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return HandleErrorResult(result);
        }

        [HttpGet("goals")] 
        public async Task<IActionResult> GetUserNutritionGoal()
        {
            var userId = GetCurrentUserId();
            var result = await _nutritionService.GetUserNutritionGoalAsync(userId);
            if (result.IsSuccess) return Ok(result.Value);
            return HandleErrorResult(result);
        }

        [HttpPost("goals")] 
        public async Task<IActionResult> SetUserNutritionGoal([FromBody] SetUserNutritionGoalDto setUserNutritionGoalDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelStateIfApplicable());
            var userId = GetCurrentUserId();
            var result = await _nutritionService.SetUserNutritionGoalAsync(setUserNutritionGoalDto, userId);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return HandleErrorResult(result);
        }

        [HttpPost("log")] 
        public async Task<IActionResult> LogFood([FromBody] LogFoodItemRequestDto logFoodDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelStateIfApplicable());
            var userId = GetCurrentUserId();
            var result = await _nutritionService.LogFoodAsync(logFoodDto, userId);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value); 
            return HandleErrorResult(result);
        }

        [HttpDelete("log/{loggedItemId:int}")] 
        public async Task<IActionResult> RemoveLoggedFood(int loggedItemId)
        {
            var userId = GetCurrentUserId();
            var result = await _nutritionService.RemoveLoggedFoodAsync(loggedItemId, userId);
            if (result.IsSuccess) return NoContent();
            return HandleErrorResult(result);
        }

        [HttpGet("log/daily")] 
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date) 
        {
            var queryDate = date ?? DateTime.Today; 
            var userId = GetCurrentUserId();
            var result = await _nutritionService.GetDailyNutritionSummaryAsync(userId, queryDate);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return HandleErrorResult(result);
        }

        [HttpGet("log/history")] 
        public async Task<IActionResult> GetUserLoggedFoodHistory(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null, 
            [FromQuery] string? mealContextFilter = null)
        {
            var userId = GetCurrentUserId();
            var result = await _nutritionService.GetUserLoggedFoodHistoryAsync(userId, pageNumber, pageSize, startDate, endDate, mealContextFilter);
            if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            return HandleErrorResult(result);
        }
    }
}