using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TradeHub.API.Extensions;

namespace TradeHub.API.Filters
{
    /// <summary>
    /// Filter tự động kiểm tra tính hợp lệ của dữ liệu đầu vào (ModelState).
    /// Việc tập trung logic kiểm tra tại đây giúp loại bỏ việc viết lặp lại các đoạn mã 
    /// !ModelState.IsValid trong từng Action của Controller.
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Nếu dữ liệu không hợp lệ, ngay lập tức chặn yêu cầu và trả về lỗi chuẩn.
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    context.ModelState.ToApiResponse()
                );
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Không thực hiện hành động sau khi action chạy xong.
        }
    }
}