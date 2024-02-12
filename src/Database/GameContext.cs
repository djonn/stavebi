using Microsoft.EntityFrameworkCore;
using StaveBi.Model;

namespace StaveBi.Database;

public class GameContext : DbContext
{
  protected readonly IConfiguration Configuration;

  public GameContext(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder options)
  {
    options.UseSqlite(Configuration.GetConnectionString("db"));
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Word>().HasKey(x => x.Value);
    modelBuilder.Entity<Word>().HasIndex(x => x.Value);
  }

  public DbSet<Word> Words { get; set; }
}