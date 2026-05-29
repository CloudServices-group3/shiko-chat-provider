using Microsoft.EntityFrameworkCore;

namespace Shiko.ChatProvider.API.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // keeping this in case there is time to modify chat to integrate db
    }
}