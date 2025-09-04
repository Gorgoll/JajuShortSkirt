using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace JajuShortSkirt.Database;
public class ApplicationDbContext : DbContext
{
    public DbSet<CoinBalance> CoinBalance { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

}
public class CoinBalance
{ 
    [Key] 
    public ulong DiscordUserId { get; set; }
    public int Balance { get; set; }
    public DateTime LastWorked { get; set; } 
}