using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;
        

        public SQLEmployeeRepository(AppDbContext context)
        {
            this.context = context;
            
        }
        public Employees Add(Employees employees)
        {
            context.Employees.Add(employees);
            context.SaveChanges();
            return employees;
        }

        public Employees Delete(int id)
        {
            Employees e = context.Employees.Find(id);
            if (e != null)
            {
                context.Employees.Remove(e);
                context.SaveChanges();
            }
            return e;
        }

        public IEnumerable<Employees> getAllEmployees()
        {
            return context.Employees;
        }

        public Employees GetEmployee(int employeeID)
        {
            return context.Employees.Find(employeeID);

        }

        public Employees Update(Employees employees)
        {
            
            var employee = context.Employees.Attach(employees);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return employees;
        }
    }
}
