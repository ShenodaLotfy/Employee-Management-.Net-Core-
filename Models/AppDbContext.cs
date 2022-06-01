using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    // by default IndentityDbContext is like IdentityDContect<IdentityUser> so we change it to our ExtendIdentityUser
    public class AppDbContext : IdentityDbContext<ExtendIdentityUser> //DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Employees> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // to seed a default data for the application
        {
            //modelBuilder.Entity<Employees>().HasData(
            //        new Employees
            //        {
            //            id = 1,
            //            name = "Shenoda",
            //            email = "Shenoda@gmail.com",
            //            Department = Dept.IT
            //        },
            //         new Employees
            //         {
            //             id = 2,
            //             name = "Mena",
            //             email = "Mena@gmail.com",
            //             Department = Dept.CS
            //         }

            //    ) ;

            // we can use extension method to make this class clean - we make ModelExtensions class 
            base.OnModelCreating(modelBuilder); // to avoid error while AddMigration migration, we send modelBuilder to base IdentityDbContext instead of DbContext 
            modelBuilder.SeedData();

            foreach(var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e=>e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
