using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace HearYe.Shared
{
    public class HearYeContext : DbContext
    {
        public HearYeContext(DbContextOptions<HearYeContext> options) : base(options) 
        { 
        }

        public DbSet<User>? Users { get; set; }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Acknowledgement>? Acknowledgements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            User demoUser = new()
            {
                Id = 1,
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"),
                DisplayName = "TestUser",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            Post demoPost1 = new()
            {
                Id = 1,
                UserId = 1,
                Message = "This is test message 1. Wow.",
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Post demoPost2 = new()
            {
                Id = 2,
                UserId = 1,
                Message = "This is test message 2. Amazing.",
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Acknowledgement demoAcknowledgement1 = new()
            {
                Id = 1,
                PostId = 1,
                UserId = 1,
                CreatedDate = DateTime.Now
            };

            Acknowledgement demoAcknowledgement2 = new()
            {
                Id = 2,
                PostId = 2,
                UserId = 1,
                CreatedDate = DateTime.Now
            };

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasData(demoUser);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.Message).HasMaxLength(255).IsRequired();
                entity.HasData(demoPost1, demoPost2);
            });

            modelBuilder.Entity<Acknowledgement>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Post)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.PostId);

                entity.HasData(demoAcknowledgement1, demoAcknowledgement2);
            });
        }
    }
}
