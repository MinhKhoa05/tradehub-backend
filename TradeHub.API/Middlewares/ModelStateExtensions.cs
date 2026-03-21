using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TradeHub.API.Extensions
{
    public static class ModelStateExtensions
    {
        public static ApiResponse ToApiResponse(this ModelStateDictionary modelState)
        {
            var firstError = modelState
                .Values
                .SelectMany(v => v.Errors)
                .FirstOrDefault()?.ErrorMessage;

            return ApiResponse.Fail(firstError ?? "Dữ liệu không hợp lệ");
        }
    }
}