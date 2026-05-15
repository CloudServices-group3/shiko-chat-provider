using Shiko.ChatProvider.API.Dtos;

namespace Shiko.ChatProvider.API.Services;

public interface IChatRoomService
{
    // Create a new chat room for a course if it doesn't exist, otherwise return the existing one
    Task<ChatRoomDto> GetOrCreateChatRoomAsync(Guid courseId);

   

    // Get or create an ACS identity for a user + return the token
    Task<AcsTokenDto> GetOrCreateAcsTokenAsync(string userId);
}
