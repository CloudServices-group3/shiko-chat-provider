using Microsoft.EntityFrameworkCore;

namespace Shiko.ChatProvider.API.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Tom nu eftersom vi inte sparar chattrådar eller ACS-användare i DB längre
    }
}