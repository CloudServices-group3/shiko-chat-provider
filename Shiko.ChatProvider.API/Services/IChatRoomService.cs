using Shiko.ChatProvider.API.Dtos;

namespace Shiko.ChatProvider.API.Services;

public interface IChatRoomService
{
    /// <summary>
    /// Get already existing or create a new chat room for the given course.
    /// Also ensures that the user is added as a participant to the thread using the Admin client.
    /// </summary>
    Task<ChatRoomDto> GetOrCreateChatRoomAsync(Guid courseId, string acsUserId, string username);

    /// <summary>
    /// Create ACS identity for the user. 
    /// </summary>
    Task<string> CreateAcsIdentityAsync();

    /// <summary>
    /// Generate a new ACS token for the given user. This is used when the user joins the chat to ensure they have a valid token.
    /// </summary>
    Task<AcsTokenDto> GenerateChatTokenAsync(string appUserId, string existingAcsUserId);
}
