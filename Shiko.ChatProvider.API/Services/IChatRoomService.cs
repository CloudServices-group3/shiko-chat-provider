using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Services;


    public interface IChatRoomService
    {
    //Task<object> SetupGlobalChatAsync(); //call once to setup global thread and admin user
    Task<ChatRoomResponseDto> JoinGlobalChatAsync(string username);
    }
 