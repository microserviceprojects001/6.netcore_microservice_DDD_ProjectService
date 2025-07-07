

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace User.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _hostEnvironment;

    private readonly ILogger<GlobalExceptionFilter> _logger;
    public GlobalExceptionFilter(IHostEnvironment hostEnvironment, ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public void OnException(ExceptionContext context)
    {
        var json = new JsonErrorResponse();
        if (context.Exception is UserOperationException userException)
        {
            json.Message = userException.Message;
            context.Result = new BadRequestObjectResult(json);
        }
        else
        {
            json.Message = "An unexpected error occurred.";
            if (_hostEnvironment.IsDevelopment())
            {
                json.DeveloperMessage = context.Exception.StackTrace;
            }

            context.Result = new InternalServerErrorResult(json);
        }
        _logger.LogError(context.Exception, context.Exception.Message);
        context.ExceptionHandled = true; // Mark the exception as handled

    }
}

public class InternalServerErrorResult : ObjectResult
{
    public InternalServerErrorResult(object value) : base(value)
    {
        StatusCode = StatusCodes.Status500InternalServerError; // Internal Server Error
    }
}