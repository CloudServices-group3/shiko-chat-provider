namespace Shiko.ChatProvider.API.Models;

public class ChatRoomResponseDto
{
    public string AzureThreadId { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string AcsUserId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTimeOffset ExpiresOn { get; set; }
}