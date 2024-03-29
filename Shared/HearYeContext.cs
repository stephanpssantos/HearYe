﻿using System;
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
        public DbSet<MessageGroup>? MessageGroups { get; set; }
        public DbSet<MessageGroupMember>? MessageGroupMembers { get; set; }
        public DbSet<MessageGroupRole> MessageGroupRoles { get; set; }
        public DbSet<MessageGroupInvitation> MessageGroupInvitations { get; set; }
        public DbSet<MessageGroupShortcut> MessageGroupShortcuts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.AadOid).IsUnique();

                entity.HasOne(e => e.DefaultGroup)
                    .WithMany()
                    .HasForeignKey(e => e.DefaultGroupId)
                    .OnDelete(DeleteBehavior.SetNull); // Set User.DefaultGroup to null when MessageGroup is deleted.
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull); // Set Post.User to null when User is deleted.

                entity.HasOne(e => e.MessageGroup)
                    .WithMany(parent => parent.Posts)
                    .HasForeignKey(e => e.MessageGroupId)
                    .OnDelete(DeleteBehavior.NoAction); // Do nothing if MessageGroup deleted.

                entity.Property(p => p.Message).HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<Acknowledgement>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete Acknowledgement if Acknowledgement.User is deleted.

                entity.HasOne(e => e.Post)
                    .WithMany(parent => parent.Acknowledgements)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete Acknowledgement if Acknowledgement.Post is deleted.

                entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique(); // Prevent same user same post.
            });

            modelBuilder.Entity<MessageGroupMember>(entity =>
            {
                entity.HasOne(e => e.MessageGroup)
                    .WithMany(parent => parent.MessageGroupMembers)
                    .HasForeignKey(e => e.MessageGroupId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupMember if MessageGroup is deleted.

                entity.HasOne(e => e.MessageGroupRole)
                    .WithMany(parent => parent.MessageGroupMembers)
                    .HasForeignKey(e => e.MessageGroupRoleId)
                    .OnDelete(DeleteBehavior.SetNull); // Set MessageGroupMember.Role to null if MessageGroupRole is deleted.

                entity.HasOne(e => e.User)
                    .WithMany(parent => parent.MessageGroups)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupMember if User is deleted.

                entity.HasIndex(e => new { e.UserId, e.MessageGroupId }).IsUnique(); // Prevent same user same group.
            });

            modelBuilder.Entity<MessageGroupInvitation>(entity =>
            {
                entity.HasOne(e => e.MessageGroup)
                    .WithMany(parent => parent.MessageGroupInvitations)
                    .HasForeignKey(e => e.MessageGroupId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupInvitation if MessageGroup is deleted.

                entity.HasOne(e => e.InvitingUser)
                    .WithMany(parent => parent.MessageGroupInvitationsSent)
                    .HasForeignKey(e => e.InvitingUserId)
                    .OnDelete(DeleteBehavior.NoAction); // Do nothing if InvitingUser is deleted.

                entity.HasOne(e => e.InvitedUser)
                    .WithMany(parent => parent.MessageGroupInvitations)
                    .HasForeignKey(e => e.InvitedUserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupInvitation if InvitedUser is deleted.

                entity.HasIndex(e => 
                    new { 
                        e.MessageGroupId, 
                        e.InvitedUserId, 
                        e.InvitationActive 
                    }).IsUnique(); // Prevent same user same group.
            });

            modelBuilder.Entity<MessageGroupRole>(entity =>
            {
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            modelBuilder.Entity<MessageGroupShortcut>(entity =>
            {
                entity.HasOne(e => e.MessageGroup)
                    .WithMany()
                    .HasForeignKey(e => e.MessageGroupId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupShortcut if MessageGroup is deleted.

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete MessageGroupShortcut if User is deleted.

                entity.HasIndex(e => new { e.UserId, e.MessageGroupId }).IsUnique(); // Prevent same user same group.
            });
        }
    }
}
