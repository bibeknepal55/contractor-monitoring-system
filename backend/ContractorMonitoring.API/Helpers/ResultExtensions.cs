using Microsoft.AspNetCore.Mvc;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.API.Helpers;

// Maps ApiResponse<T> to proper HTTP status codes based on message content
public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this ApiResponse<T> result)
    {
        if (result.Success)
            return new OkObjectResult(result);

        var message = result.Message?.ToLower() ?? "";

        return message switch
        {
            _ when message.Contains("not found") => new NotFoundObjectResult(result),
            _ when message.Contains("unauthorized") || message.Contains("invalid credentials") || message.Contains("password") => new UnauthorizedObjectResult(result),
            _ when message.Contains("permission") || message.Contains("access denied") || message.Contains("cannot") || message.Contains("not allowed") => new ObjectResult(result) { StatusCode = 403 },
            _ when message.Contains("already exists") || message.Contains("duplicate") => new ConflictObjectResult(result),
            _ => new BadRequestObjectResult(result)
        };
    }
}