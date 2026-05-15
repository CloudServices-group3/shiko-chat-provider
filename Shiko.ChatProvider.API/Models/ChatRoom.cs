namespace Shiko.ChatProvider.API.Models;

public class ChatRoom
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    // Id from azure as a string
    public string AzureThreadId { get; set; } = null!;

    public DateTime Created { get; set; }
}
