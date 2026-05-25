using Azure;
using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Services;

public class ChatRoomService(
    ChatDbContext context,
    IConfiguration config
) : IChatRoomService
{
    private readonly string _acsEndpoint =
        config["AzureCommunicationServices:Endpoint"]
        ?? throw new InvalidOperationException(
            "Azure Communication Services endpoint is missing."
        );

    // handle ACS identities and tokens (shared by both Admin and User clients)
    private readonly CommunicationIdentityClient _identityClient =
        new(
            config["AzureCommunicationServices:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Azure Communication Services connection string is missing."
            )
        );

    // ADMIN chat client
    private readonly ChatClient _adminChatClient = new(
        new Uri(config["AzureCommunicationServices:Endpoint"] ?? throw new InvalidOperationException("Azure Communication Services endpoint is missing.")),
        new CommunicationTokenCredential(
            new CommunicationIdentityClient(
                config["AzureCommunicationServices:ConnectionString"] ?? throw new InvalidOperationException("Azure Communication Services connection string is missing.")
            ).CreateUserAndToken(scopes: [CommunicationTokenScope.Chat]).Value.AccessToken.Token
        )
    );

    /// <summary>
    /// Gets an existing chat room or creates a new one.
    /// Ensures the user is added to the thread using the Admin client.
    /// </summary>
    public async Task<ChatRoomDto> GetOrCreateChatRoomAsync(
        Guid courseId,
        string acsUserId,
        string username)
    {
        // Check if room already exists
        var chatRoomEntity = await context.ChatRooms
            .FirstOrDefaultAsync(x => x.CourseId == courseId);

        string azureThreadId;

        var participant = new ChatParticipant(
            new CommunicationUserIdentifier(acsUserId))
        {
            DisplayName = username
        };

        // =========================================================
        // CREATE NEW ROOM (If missing)
        // =========================================================
        if (chatRoomEntity == null)
        {
            var createThreadResult =
                await _adminChatClient.CreateChatThreadAsync(
                    topic: $"Chat for Course {courseId}",
                    participants: [participant]
                );

            azureThreadId = createThreadResult.Value.ChatThread.Id;

            chatRoomEntity = new ChatRoom
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                AzureThreadId = azureThreadId,
                Created = DateTime.UtcNow
            };

            context.ChatRooms.Add(chatRoomEntity);
            await context.SaveChangesAsync();

            Console.WriteLine($"Created new chat thread: {azureThreadId}");
        }
        // =========================================================
        // EXISTING ROOM (Add participant)
        // =========================================================
        else
        {
            azureThreadId = chatRoomEntity.AzureThreadId;
            var threadClient = _adminChatClient.GetChatThreadClient(azureThreadId);

            try
            {
                await threadClient.AddParticipantsAsync([participant]);
                Console.WriteLine($"Admin added participant {acsUserId} to thread {azureThreadId}");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Could not add participant (might already be in thread): {ex.Message}");
            }
        }

        return new ChatRoomDto
        {
            Id = chatRoomEntity.Id,
            CourseId = chatRoomEntity.CourseId,
            AzureThreadId = chatRoomEntity.AzureThreadId,
            Created = chatRoomEntity.Created
        };
    }

    /// <summary>
    /// Create ACS identity ONCE per app user.
    /// Save the returned AcsUserId in your database.
    /// </summary>
    public async Task<string> CreateAcsIdentityAsync()
    {
        var response = await _identityClient.CreateUserAsync();
        return response.Value.Id;
    }

    /// <summary>
    /// Generate a fresh token for an existing ACS user.
    /// </summary>
    public async Task<AcsTokenDto> GenerateChatTokenAsync(
        string appUserId,
        string existingAcsUserId)
    {
        var tokenResponse = await _identityClient.GetTokenAsync(
            new CommunicationUserIdentifier(existingAcsUserId),
            [CommunicationTokenScope.Chat]
        );

        return new AcsTokenDto
        {
            UserId = appUserId,
            AcsUserId = existingAcsUserId,
            Token = tokenResponse.Value.Token,
            ExpiresOn = tokenResponse.Value.ExpiresOn,
            Endpoint = _acsEndpoint
        };
    }
}
