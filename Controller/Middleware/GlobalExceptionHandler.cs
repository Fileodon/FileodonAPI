using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Middleware;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetailsContext problemDetailsContext;
        // If we're in development, provide more details about the error
        if (httpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
        {
            string[] stackTrace = exception.StackTrace?.Split(
                ["\r\n", "\n"],
                StringSplitOptions.RemoveEmptyEntries
            ) ?? [];

            problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = httpContext.Response.StatusCode,
                    Title = exception.Message,
                    Detail = "See the stack trace extension for more details.",
                    Extensions = { ["stackTrace"] = stackTrace,
                                   ["exceptionType"] = exception.GetType().FullName }
                },
                Exception = exception,
            };
        }
        else
        {
            problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = httpContext.Response.StatusCode,
                    Title = "An unexpected error occurred.",
                    Detail = "Please contact support or try again later."
                },
                Exception = exception,
            };
        }

        return await _problemDetailsService.TryWriteAsync(problemDetailsContext);
    }
}
