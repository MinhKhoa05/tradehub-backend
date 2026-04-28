using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TradeHub.API.Extensions
{
    /// <summary>
    /// Các phương thức mở rộng cho ModelStateDictionary để chuyển đổi lỗi validation sang ApiResponse.
    /// </summary>
    public static class ModelStateExtensions
    {
        public static ApiResponse ToApiResponse(this ModelStateDictionary modelState)
        {
            // Trích xuất thông báo lỗi đầu tiên để phản hồi cho người dùng một cách ngắn gọn.
            var firstError = modelState
                .Values
                .SelectMany(v => v.Errors)
                .FirstOrDefault()?.ErrorMessage;

            return ApiResponse.Fail(firstError ?? "Dữ liệu đầu vào không hợp lệ. Vui lòng kiểm tra lại.");
        }
    }
}