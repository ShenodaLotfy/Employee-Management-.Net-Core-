using NetCore_Model_View_Cortol_Created.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class HomeDetailsViewModel // used from the controller to send both Employee and String value together to View
    {
        public Employees Employee { get; set; }
        public string PageTitle { get; set; }
    }
}
