using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyFlowPro.Web.Models;

namespace StudyFlowPro.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(120);
        });

        builder.Entity<Subject>(entity =>
        {
            entity.HasIndex(subject => new { subject.OwnerId, subject.Code })
                .IsUnique();

            entity.HasIndex(subject => new { subject.OwnerId, subject.Name });

            entity.Property(subject => subject.AccentColor)
                .HasMaxLength(7);

            entity.HasOne(subject => subject.Owner)
                .WithMany(owner => owner.Subjects)
                .HasForeignKey(subject => subject.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudyTask>(entity =>
        {
            entity.HasIndex(task => new { task.OwnerId, task.Status, task.DueDate });

            entity.Property(task => task.EstimatedHours)
                .HasPrecision(4, 1);

            entity.HasOne(task => task.Owner)
                .WithMany(owner => owner.StudyTasks)
                .HasForeignKey(task => task.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(task => task.Subject)
                .WithMany(subject => subject.StudyTasks)
                .HasForeignKey(task => task.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
