using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ForumSite.Models;

public partial class ForumContext : DbContext
{
    public ForumContext()
    {
    }

    public ForumContext(DbContextOptions<ForumContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Friend> Friends { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<RelationType> RelationTypes { get; set; }

    public virtual DbSet<TopicDiscussion> TopicDiscussions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=ACERAZE\\MYFIRSTSERVER;Initial Catalog=Forum;User ID=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.IdAdmin).HasName("PK__admins__89472E958C149B4F");

            entity.ToTable("admins");

            entity.Property(e => e.IdAdmin).HasColumnName("id_admin");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Admins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__user_id__4F7CD00D");
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            entity.HasKey(e => e.IdFriends).HasName("PK__friends__33B1FF4445CE1054");

            entity.ToTable("friends");

            entity.Property(e => e.IdFriends).HasColumnName("id_friends");
            entity.Property(e => e.FriendId).HasColumnName("friend_id");
            entity.Property(e => e.RelationId).HasColumnName("relation_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FriendNavigation).WithMany(p => p.FriendFriendNavigations)
                .HasForeignKey(d => d.FriendId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__friends__friend___5CD6CB2B");

            entity.HasOne(d => d.Relation).WithMany(p => p.Friends)
                .HasForeignKey(d => d.RelationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__friends__relatio__5DCAEF64");

            entity.HasOne(d => d.User).WithMany(p => p.FriendUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__friends__user_id__5BE2A6F2");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PK__message__460F3CF4D6C68EF9");

            entity.ToTable("message");

            entity.Property(e => e.IdMessage).HasColumnName("id_message");
            entity.Property(e => e.Rating)
                .HasDefaultValueSql("((0))")
                .HasColumnName("rating");
            entity.Property(e => e.StatusChange)
                .HasDefaultValueSql("((0))")
                .HasColumnName("status_change");
            entity.Property(e => e.TextMessage)
                .HasColumnType("text")
                .HasColumnName("text_message");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Topic).WithMany(p => p.Messages)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__message__topic_i__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__message__user_id__5812160E");
        });

        modelBuilder.Entity<RelationType>(entity =>
        {
            entity.HasKey(e => e.IdRelation).HasName("PK__relation__EBAB3EF1F4BBA5DA");

            entity.ToTable("relation_type");

            entity.Property(e => e.IdRelation).HasColumnName("id_relation");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TopicDiscussion>(entity =>
        {
            entity.HasKey(e => e.IdTopic).HasName("PK__topic_di__13CAB20C301D4A47");

            entity.ToTable("topic_discussion");

            entity.Property(e => e.IdTopic).HasColumnName("id_topic");
            entity.Property(e => e.DateCreation)
                .HasColumnType("date")
                .HasColumnName("date_creation");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name).IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TopicDiscussions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__topic_dis__user___534D60F1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__users__D2D14637EB49C0EF");

            entity.ToTable("users");

            entity.HasIndex(e => e.Login, "UQ__users__7838F2725CD87C19").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.DateRegistration)
                .HasColumnType("date")
                .HasColumnName("date_registration");
            entity.Property(e => e.Email)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fio)
                .IsUnicode(false)
                .HasColumnName("FIO");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Block)
                .HasColumnType("bit")
                .HasColumnName("block");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
