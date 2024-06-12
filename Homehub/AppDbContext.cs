using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homehub
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer("Server=desktop-1nft1om;" +
           "Database=Homehub; Integrated Security=SSPI;" +
           "TrustServerCertificate=true");
        }


       public DbSet<Product> Products { get; set; }


    }
}
