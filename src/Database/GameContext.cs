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
    modelBuilder.Entity<WordDetails>().HasKey(x => x.Id);
    modelBuilder.Entity<WordDetails>().HasIndex(x => x.Id);
    modelBuilder.Entity<WordDetails>()
                .Property(x => x.Conjugation)
                .HasConversion(
                  c => string.Join(".", c),
                  c => new HashSet<string>(c.Split(".", StringSplitOptions.None))
                );

    modelBuilder.Entity<Game>().HasKey(x => x.Letters);
    modelBuilder.Entity<Game>().HasIndex(x => x.Letters);
  }

  public DbSet<WordDetails> Words { get; set; }
  public DbSet<Game> Games { get; set; }
}