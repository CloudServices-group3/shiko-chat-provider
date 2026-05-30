
using Shiko.ChatProvider.API.Models;
using Shiko.ChatProvider.API.Services;
using System.Security.Claims;


namespace Shiko.ChatProvider.API.Endpoints;


public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/chat")
            .WithSummary("Joins the global chat room and returns ACS credentials")
           .RequireAuthorization();

        group.MapPost("/join/{courseId}", JoinChat)
             .WithName("JoinChatRoom")
             .WithDescription("Creates a unique session token for Azure Communication " +
             "Services (ACS) and adding user to global chat thread")
             .Produces<ChatRoomResponseDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status500InternalServerError);

    }

    private static async Task<IResult> JoinChat(
            Guid courseId,
            HttpContext httpContext,
            IChatRoomService chatService,
            ILogger <ChatRoomService> logger
        )
    {
        try
        {     // get user email from JWT-token in frontend request, if not found, use "Class mate" as default username
            var username = httpContext.User.FindFirst("email")?.Value
                  ?? httpContext.User.FindFirst(ClaimTypes.Email)?.Value
                  ?? "Class mate";

            logger.LogInformation("User {Username} attempting to join chat", username);

            // call method to join global chat and get credentials
            var response = await chatService.JoinGlobalChatAsync(username);

            logger.LogInformation("User {Username} successfully joined chat", username);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to join chat for user");
            return Results.Problem("Something went wrong joining the chat.", statusCode: 500);
        }
    }
}