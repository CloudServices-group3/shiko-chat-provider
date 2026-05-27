
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
             .WithName("JoinChatRoom");
        
    }


    private static async Task<IResult> JoinChat(
            Guid courseId,
            HttpContext httpContext,
            IChatRoomService chatService)
        {
            try
            {     
                var username = httpContext.User.FindFirst("email")?.Value
                  ?? httpContext.User.FindFirst(ClaimTypes.Email)?.Value
                  ?? "Class mate";

               // call method to join global chat and get credentials
               var response = await chatService.JoinGlobalChatAsync(username);

                
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHATT-ERROR]: {ex.Message}");
                return Results.Json(new { error = ex.Message }, statusCode: 500);
            }
        }
    }