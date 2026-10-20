using ConsoleApp14.Models;
using Microsoft.EntityFrameworkCore;

class ApplicationDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
}

