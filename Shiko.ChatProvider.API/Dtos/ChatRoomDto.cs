namespace Shiko.ChatProvider.API.Dtos;

public class ChatRoomDto
{
    // Id from my DB
    public Guid Id { get; set; }

    // Id from course used to link the chat room to a specific course
    public Guid CourseId { get; set; }

    // Id for the ACS thread 
    public string AzureThreadId { get; set; } = null!;

    public DateTime Created { get; set; }
}
