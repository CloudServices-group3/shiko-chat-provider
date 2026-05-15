namespace Shiko.ChatProvider.API.Dtos;

public class AcsTokenDto
{
    // user id from auth
    public string UserId { get; set; } = null!;

    // id from Azure Communication Services for the specific user
    public string AcsUserId { get; set; } = null!;

    // the JWT-token allowing the user to enter chat
    public string Token { get; set; } = null!;

    // Token Expiration
    public DateTimeOffset ExpiresOn { get; set; }

    // endpoint to Azure Communication Service 
    public string Endpoint { get; set; } = null!;
}