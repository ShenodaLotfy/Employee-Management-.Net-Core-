using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class EmployeeEditViewModel : PhotoViewModel // inherit because when we gonna to edit employee we need all the same properties in PhotoViewModel
    {
        public string ExistingPhotoPath { get; set; }
        public int Id { get; set; }
    }
}
