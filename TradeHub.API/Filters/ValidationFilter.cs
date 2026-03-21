using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TradeHub.API.Extensions;

namespace TradeHub.API.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    context.ModelState.ToApiResponse()
                );
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}