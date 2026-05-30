
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Services;


    public interface IChatRoomService
    {
    /// <summary>
    /// Handles connection to the global chat thread. Each user gets a unique ACS identity and token,
    /// and is added as a participant to the global thread.
    /// Will return a chatRoomResponseDto with authentication details to the frontend.
    /// </summary>
    /// <param name="username">Email or username to be displayed in the chat</param>
    /// <returns> <see cref="ChatRoomResponseDto"/> dto containing ACS-token, endpoint and global thread-ID:t.</returns
    Task<ChatRoomResponseDto> JoinGlobalChatAsync(string username);
    }
 