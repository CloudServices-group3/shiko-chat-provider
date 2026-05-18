using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Shiko.ChatProvider.API.Middleware;

// global exception handler to handle unexpected errors. implementing the built in .NET IExceptionHandler
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        httpContext.Response.StatusCode = exception switch
        {
            // _ = for all errors -> send a 500InternalServerError
            _ => StatusCodes.Status500InternalServerError
        };

        //return json object to frontend, and not the stack trace

        return await httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = httpContext.Response.StatusCode,
                    Title = "An unexpected error occurred",
                    Detail = "Something went wrong in the system. Please try again later"
                }
            });
    }
}
