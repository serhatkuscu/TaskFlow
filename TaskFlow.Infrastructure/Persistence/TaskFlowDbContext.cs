using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

public class TaskFlowDbContext : DbContext
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options)
        : base(options)
    {
    }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.Description)
                  .HasMaxLength(1000);

            entity.Property(x => x.Status)
                  .IsRequired();

            entity.Property(x => x.UserId)
                  .IsRequired();

            // Each task belongs to exactly one user.
            // No navigation property on TaskItem — configured entirely here.
            // Cascade: deleting a user removes all their tasks.
            entity.HasOne<AppUser>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasIndex(x => x.Username)
                  .IsUnique();

            entity.Property(x => x.PasswordHash)
                  .IsRequired();

            entity.Property(x => x.Role)
                  .IsRequired()
                  .HasMaxLength(50);
        });
    }

}