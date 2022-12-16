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
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete post if user is deleted.

                entity.Property(p => p.Message).HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<Acknowledgement>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.Post)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.PostId);
            });
        }
    }
}
