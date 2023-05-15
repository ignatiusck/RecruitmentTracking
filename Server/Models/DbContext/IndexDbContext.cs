using Microsoft.EntityFrameworkCore;
using System.Data.SQLite;

namespace Server.Models;

public class IndexDbContext : DbContext
{
    public DbSet<Admin>? Admins { get; set; }
    public DbSet<Candidate>? Candidates { get; set; }
    public DbSet<Jobs>? Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=./Models/DbContext/Data.db");
    }
}
