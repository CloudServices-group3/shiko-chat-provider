namespace Shiko.ChatProvider.API.Dtos;

public class JoinChatResponseDto
{
    public ChatRoomDto Room { get; set; } = null!;
    public AcsTokenDto TokenData { get; set; } = null!;
}
