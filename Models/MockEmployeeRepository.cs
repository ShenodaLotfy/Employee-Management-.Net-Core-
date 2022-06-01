using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    public class MockEmployeeRepository /* class for implementation */ : IEmployeeRepository // to implements its method
    {
        private List<Employees> employeesList;
        public MockEmployeeRepository()
        {
            employeesList = new List<Employees>();
            employeesList.Add(new Employees() { id = 1, name = "Shenoda", email = "Shenoda@gamil.com", Department = Dept.IT });
            employeesList.Add(new Employees() { id = 2, name = "Mena", email = "Mean@gamil.com", Department = Dept.CS });
            employeesList.Add(new Employees() { id = 3, name = "Kero", email = "Kero@gamil.com", Department = Dept.IS });
            employeesList.Add(new Employees() { id = 4, name = "Sara", email = "Sara@gamil.com", Department = Dept.CS });
        }

        public Employees Add(Employees employees) // add employee and return employee - used to create a new employee within Home Controller Create() method
        {
            employees.id = employeesList.Max(e => e.id) + 1;  // we adding emloyee(name,email,department) without ID, so now we are making ID to the new employee to be added
            employeesList.Add(employees);
            return employees;
        }

        public Employees Delete(int id)
        {
            Employees emp = employeesList.FirstOrDefault(employee => employee.id == id);
            if(emp != null)
            {
                employeesList.Remove(emp);
            }
            return emp;
        }

        public IEnumerable<Employees> getAllEmployees() // method to get all employees - implementation for IEmployeeRepository
        {
            var allEmployees = employeesList; // assign the list to variable
            return allEmployees; // then send this IEnumerable variable
        }

        public Employees GetEmployee(int employeeID) // this method is an implementation of interface declaration IEmployeeRepository
        {
            // implementation of interface IEmplyeeRepository to help in dependency Injection
            return employeesList.FirstOrDefault(employee => employee.id == employeeID);
        }

        public Employees Update(Employees employees)
        {
            Employees emp = employeesList.FirstOrDefault(employee => employee.id == employees.id);
            if(emp != null)
            {
                emp.name = employees.name;
                emp.email = employees.email;
                emp.Department = employees.Department;
            }
            return emp;
        }
    }
}
