using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Shiko.ChatProvider.API.Services;

public class ChatRoomService : IChatRoomService
{
    private readonly ChatClient _chatClient;
    private readonly ChatDbContext _context;
    private readonly CommunicationIdentityClient _identityClient;
    private readonly string _acsEndpoint;

    public ChatRoomService(ChatClient chatClient, ChatDbContext context, IConfiguration config)
    {
        _chatClient = chatClient;
        _context = context;

        // get connection string to create IdentityClient for token generation
        var connectionString = config.GetConnectionString("AzureCommunicationServices")
            ?? throw new ArgumentNullException("Cannot find connection string for Azure Communication Services");

        _identityClient = new CommunicationIdentityClient(connectionString);

        // get URL from endpoint in connection string
        _acsEndpoint = connectionString.Split(';')[0].Replace("endpoint=", "");
    }

    /// <summary>
    ///Get chat room from db.  If not existing, create a new thread in Azure and save it in db.
    /// </summary>
    public async Task<ChatRoomDto> GetOrCreateChatRoomAsync(Guid courseId)
    {
        // Check if chat room alread exists for the course 
        var chatRoomEntity = await _context.ChatRooms
            .FirstOrDefaultAsync(x => x.CourseId == courseId);

        // If chat room doesn't exist (Lazy Creation)
        if (chatRoomEntity == null)
        {
            // call Azure SDK to create new chat thread and get threadId
            var createChatThreadResult = await _chatClient.CreateChatThreadAsync(topic: $"Chat for Course {courseId}");
            var azureThreadId = createChatThreadResult.Value.ChatThread.Id;

            // create entity and save to db
            chatRoomEntity = new ChatRoom
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                AzureThreadId = azureThreadId,
                Created = DateTime.UtcNow
            };

            _context.ChatRooms.Add(chatRoomEntity);
            await _context.SaveChangesAsync();
        }

        // map to DTO and return
        return new ChatRoomDto
        {
            Id = chatRoomEntity.Id,
            CourseId = chatRoomEntity.CourseId,
            AzureThreadId = chatRoomEntity.AzureThreadId,
            Created = chatRoomEntity.Created
        };
    }

    /// <summary>
    /// Creates a unique Azure-identity and a time-limited token for the user.
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