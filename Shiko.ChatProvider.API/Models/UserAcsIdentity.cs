namespace Shiko.ChatProvider.API.Models;

public class UserAcsIdentity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;  // from auth
    public string AcsUserId { get; set; } = null!;  // from azure
    public DateTime Created { get; set; }
}
