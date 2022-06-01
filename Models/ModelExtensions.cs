using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    
    public static class ModelExtensions
    {
        // extension method to seed data while running the application 
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employees>().HasData( // seed data to Employees entity
                    new Employees
                    {
                        id = 1,
                        name = "Shenoda",
                        email = "Shenoda@gmail.com",
                        Department = Dept.IT
                    },
                     new Employees
                     {
                         id = 2,
                         name = "Mena",
                         email = "Mena@gmail.com",
                         Department = Dept.CS
                     }

                );
            // to apply seeding data, add migration 
        }
    }
}
