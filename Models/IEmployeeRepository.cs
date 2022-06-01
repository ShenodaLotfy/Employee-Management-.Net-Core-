using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    public interface IEmployeeRepository
    {
        Employees GetEmployee(int employeeID);

        IEnumerable<Employees> getAllEmployees();
        Employees Add(Employees employees);

        Employees Update(Employees employees);
        Employees Delete(int id);

    }
}
