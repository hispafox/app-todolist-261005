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
    }
}
