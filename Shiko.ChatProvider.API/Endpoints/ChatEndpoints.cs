using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Services;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace Shiko.ChatProvider.API.Endpoints;

public static class ChatEndpoints
{
    private static readonly ConcurrentDictionary<string, string> UserAcsMapping = new();

    public static void MapChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/chat")
            .WithName("JoinChatRoom")
            .WithSummary("Joins a chat room for a specific course and returns ACS credentials")
            .RequireAuthorization();

        group.MapPost("/join/{courseId}", JoinChat);
    }

    // method to enter chat (POST /api/chat/join/{courseId})
    static async Task<IResult> JoinChat(Guid courseId, HttpContext httpContext, IChatRoomService chatService)
    {
        try
        {
            // 1. Hämta användarens fasta ID från JWT-tokenet
            var userId = httpContext.User.FindFirst("userId")?.Value
                      ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.User.FindFirst("sub")?.Value;

            var username = httpContext.User.FindFirst("name")?.Value
                        ?? httpContext.User.FindFirst(ClaimTypes.Name)?.Value
                        ?? "Användare";

            if (userId is null) return Results.Unauthorized();

            // 2. Kolla om denna användare REDAN har fått ett Azure-ID skapat tidigare
            if (!UserAcsMapping.TryGetValue(userId, out var acsUserId))
            {
                // Om inte -> Skapa ett helt nytt ID via Azure
                acsUserId = await chatService.CreateAcsIdentityAsync();

                // Spara det i minnet så servern kommar ihåg det vid nästa sidladdning!
                UserAcsMapping.TryAdd(userId, acsUserId);
                Console.WriteLine($"[AZURE-ACS]: Skapade NYTT ID för användare {userId}: {acsUserId}");
            }
            else
            {
                Console.WriteLine($"[AZURE-ACS]: Återanvänder BEFINTLIGT ID för användare {userId}: {acsUserId}");
            }

            // 3. Generera ett färskt token till det fasta ID:t
            var tokenData = await chatService.GenerateChatTokenAsync(userId, acsUserId);

            // 4. Hämta/Skapa rummet via Admin-klienten
            var room = await chatService.GetOrCreateChatRoomAsync(courseId, acsUserId, username);

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