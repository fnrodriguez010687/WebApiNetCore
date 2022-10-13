using Microsoft.EntityFrameworkCore;
using WebApiNetCore.Models;

namespace WebApiNetCore.Data
{
    public class DataContext : DbContext
    {
      public DataContext(DbContextOptions<DataContext> options): base(options)
      {} 
      public DbSet<Gateway> Gateways { get; set; }
      public DbSet<User> Users { get; set; }
      public DbSet<PeripheralDevice> PeripheralDevices { get; set; }
    }
}