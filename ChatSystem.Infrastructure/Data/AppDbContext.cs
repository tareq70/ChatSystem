using ChatSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace ChatSystem.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MessageRead> MessageReads { get; set; }
        public DbSet<Group> Groups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum conversions
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<int>();

            modelBuilder.Entity<FriendRequest>()
                .Property(f => f.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Chat>()
                .Property(c => c.Type)
                .HasConversion<int>();
          
            modelBuilder.Entity<Friend>()
                .Property(f => f.friendsStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Friend>()
                .Property(f => f.UserStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Chat>()
            .Property(c => c.Type)
            .HasConversion<int>();

            // FriendRequests relationships
            modelBuilder.Entity<FriendRequest>()
                .HasOne<User>() // Sender
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne<User>() // Receiver
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Friends relationships
            modelBuilder.Entity<Friend>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friend>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatUsers relationships
            modelBuilder.Entity<ChatUser>()
                .HasOne<Chat>()
                .WithMany()
                .HasForeignKey(cu => cu.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatUser>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Messages relationships
            modelBuilder.Entity<Message>()
                .HasOne<Chat>()
                .WithMany()
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notifications relationships
            modelBuilder.Entity<Notification>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageRead>()
                .HasOne<Message>()
                .WithMany(m => m.Reads)
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MessageRead>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // prevent duplicate reads
            modelBuilder.Entity<MessageRead>()
                .HasIndex(mr => new { mr.MessageId, mr.UserId })
                .IsUnique();
            // Group relationships
            modelBuilder.Entity<Group>()
                .HasOne<Chat>()
                .WithOne()
                .HasForeignKey<Group>(g => g.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}