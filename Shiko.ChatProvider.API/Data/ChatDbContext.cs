using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<ChatRoomEntity> ChatRooms => Set<ChatRoomEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration
        modelBuilder.Entity<ChatRoomEntity>(builder =>
        {
            builder.ToTable("ChatRooms");

            builder.HasKey(x => x.Id).HasName("PK_ChatRooms_Id"); 

            // set unique ID in database when added
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.CourseId)
                .IsRequired();

            builder.Property(x => x.AzureThreadId)
               .IsRequired();

            builder.Property(x => x.Created)
                .HasDefaultValueSql("GETUTCDATE()");
         
        });
    }
}
