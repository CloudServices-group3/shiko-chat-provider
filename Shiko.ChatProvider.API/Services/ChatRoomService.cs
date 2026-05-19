using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Services;


public class ChatRoomService(ChatDbContext context, IConfiguration config) : IChatRoomService
{
    // initialize the CommunicationIdentityClient for creating users and tokens
    private readonly CommunicationIdentityClient _identityClient = new(
        config["AzureCommunicationServices:ConnectionString"]
           ?? throw new ArgumentNullException("Cannot find connection string for ACS")
    );

    private readonly string _acsEndpoint = config["AzureCommunicationServices:Endpoint"]
        ?? throw new InvalidOperationException("ACS Endpoint is missing.");


    /// <summary>
    /// Fetches an existing chat room from the database, or creates a new one in Azure and db if missing.
    /// </summary>
    public async Task<ChatRoomDto> GetOrCreateChatRoomAsync(Guid courseId, string acsUserId, string username)
    {

        // check if a chat room already exists for the given courseId
        var chatRoomEntity = await context.ChatRooms
            .FirstOrDefaultAsync(x => x.CourseId == courseId);

        // lazy creation, only create a new chat thread in Azure and a new record in db if there isn't one already for the course
        if (chatRoomEntity == null)
        {
            // create a temporary identity in Azure to authorize the ChatClient
            var identityResponse = await _identityClient.CreateUserAndTokenAsync(scopes: [CommunicationTokenScope.Chat]);

            // package the token into a credential object required by Azure SDK
            var credential = new CommunicationTokenCredential(identityResponse.Value.AccessToken.Token);

            // create a chat client with the credential and endpoint
            var chatClient = new ChatClient(new Uri(_acsEndpoint), credential);

            var firstParticipant = new ChatParticipant(new CommunicationUserIdentifier(acsUserId))
            {
                DisplayName = username
            };

            // request Azure to create a new chat thread and get the thread id
            var createChatThreadResult = await chatClient.CreateChatThreadAsync(
                    topic: $"Chat for Course {courseId}",
                    participants: [firstParticipant] 
                );
            var azureThreadId = createChatThreadResult.Value.ChatThread.Id;

            // create entity to map the course to the newly created Azure Thread ID
            chatRoomEntity = new ChatRoom
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                AzureThreadId = azureThreadId,
                Created = DateTime.UtcNow
            };

            context.ChatRooms.Add(chatRoomEntity);
            await context.SaveChangesAsync();
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
    /// Creates a unique Azure-identity and a time-limited token. 
    /// Used by the frontend to a allow a user to connect to the Azure Chat Thread. 
    /// </summary>
    public async Task<AcsTokenDto> GetOrCreateAcsTokenAsync(string userId)
    {
        // Use the "chat" scope in the token to allow the client to use it only for chat operations. 
        var identityResponse = await _identityClient.CreateUserAndTokenAsync(scopes: [CommunicationTokenScope.Chat]);

        return new AcsTokenDto
        {
            UserId = userId,
            AcsUserId = identityResponse.Value.User.Id,
            Token = identityResponse.Value.AccessToken.Token,
            ExpiresOn = identityResponse.Value.AccessToken.ExpiresOn,
            Endpoint = _acsEndpoint
        };
    }
}