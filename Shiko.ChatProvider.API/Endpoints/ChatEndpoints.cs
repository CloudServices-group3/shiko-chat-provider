using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Services;

namespace Shiko.ChatProvider.API.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/chat")
            .WithName("JoinChatRoom")
            .WithSummary("Joins a chat room for a specific course and returns ACS credentials");
           // .RequireAuthorization();

        group.MapPost("/join/{courseId}", JoinChat);
         
    }

    // method to enter chat (POST /api/chat/join/{courseId})
    static async Task<IResult> JoinChat(Guid courseId, string userId, IChatRoomService chatService)
    { try
        {
            var tokenData = await chatService.GetOrCreateAcsTokenAsync(userId);

            var room = await chatService.GetOrCreateChatRoomAsync(courseId, tokenData.AcsUserId, userId);

            // return a dto (JoinChatRespons)
            var response = new JoinChatResponseDto
            {
                Room = room,
                TokenData = tokenData
            };
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"[CHATT-ERROR]: {ex.Message}");
            return Results.Json(new { error = ex.Message }, statusCode: 500);
        }


    }
}
