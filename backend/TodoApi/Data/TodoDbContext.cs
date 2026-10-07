using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoCategory> TodoCategories => Set<TodoCategory>();
    public DbSet<TodoUser> TodoUsers => Set<TodoUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoCategory>(entity =>
        {
            entity.Property(category => category.Name).UseCollation("NOCASE");
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<TodoItem>()
            .HasOne(todo => todo.Category)
            .WithMany(category => category.TodoItems)
            .HasForeignKey(todo => todo.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TodoUser>()
            .Property(user => user.Name)
            .IsRequired();

        modelBuilder.Entity<TodoItem>()
            .HasOne(todo => todo.User)
            .WithMany(user => user.TodoItems)
            .HasForeignKey(todo => todo.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
