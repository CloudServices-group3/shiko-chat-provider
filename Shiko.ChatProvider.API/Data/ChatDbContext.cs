using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Data;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<UserAcsIdentity> UserAcsIdentities => Set<UserAcsIdentity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration
        modelBuilder.Entity<ChatRoom>(builder =>
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

        modelBuilder.Entity<UserAcsIdentity>(builder =>
        {
            builder.ToTable("UserAcsIdentities");

            builder.HasKey(x => x.Id).HasName("PK_UserAcsIdentities_Id");

            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.AcsUserId)
                .IsRequired();

            builder.Property(x => x.Created)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
