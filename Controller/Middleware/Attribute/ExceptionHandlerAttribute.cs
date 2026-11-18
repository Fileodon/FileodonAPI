using Microsoft.AspNetCore.Mvc.Filters;

namespace Controller.Middleware.Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ExceptionHandlerAttribute : System.Attribute, IAsyncActionFilter
{
    //private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExceptionHandlerAttribute> _logger;
    public ExceptionHandlerAttribute(ILogger<ExceptionHandlerAttribute> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();
        if (executedContext.Exception != null) // && !executedContext.ExceptionHandled
        {
            _logger.LogCritical(executedContext.Exception.InnerException?.Message ?? executedContext.Exception.Message);
        }
    }
}
