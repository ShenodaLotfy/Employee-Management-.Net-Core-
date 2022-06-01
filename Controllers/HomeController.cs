using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using NetCore_Model_View_Cortol_Created.Models;
using NetCore_Model_View_Cortol_Created.ViewModels;

namespace NetCore_Model_View_Cortol_Created.Controllers
{

    //[Route("Home")] // we can delete controller name and use general keyword
    //[Route("[controller]/[action]")]  // controller maps Home, action maps Details and Index methods
    [Authorize] // cant access actions - atleast u must login first
    public class HomeController : Controller
    {
        // creating a readonly instance of Interface IEmployeeRepository to inject its service in the controller
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment hostingEnvironment; // it's including the wwwroot path

        public HomeController(IEmployeeRepository employeeRepository,
                                IWebHostEnvironment hostingEnvironment) // injection constructot - inject the service of IEmployeeRepository
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }


        // to route to this details method with attribute routing
        //[Route("/Home/Details/{id?}")]  // we can delete /Home cuz we used it to route the controller
        //[Route("Details/{id?}")] // we can delete Details because we routed Controller
        //[Route("{id?}")]
        [AllowAnonymous] // this makes ability to access this action while we used [Authorize] on Controller 
        public ViewResult Details(int? id) //int? id means we can pass 0 parameters - then id will be null 
        {
            //test logging here when trying to get details
            //throw new Exception("Error in detils view ");

            Employees newEmp = _employeeRepository.GetEmployee(id??1); // if there's no id passed, then id = 1 
            //return View(newEmp); // by default view is in Views/Home, because we're in Home Controller - relative path
            //return View("SecondViews/Index.cshtml"); // to return a specified View from other file - absolute path 
            //return View("~/SecondViews/Details.cshtml"); ~ to go to root then search for SecondViews folder
            //return View("../../SecondViews"); // .. to back to upper dictionary path

            //// to send data to view using ViewData[Key]
            //// this approch is weakly typed - no compile errors happening in compile time - so its bad
            //ViewData["EmployeeFromController"] = newEmp;

            //// to send data to view using ViewBag.Key
            //ViewBag.EmployeeFromController = newEmp;

            // if we send an object doesnt contain all the data that view needs to display
            // then we then use ViewModel Class
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id??1), // if id is null - make it id = 1
                PageTitle = "Employee Details"
            };
            
            return View(homeDetailsViewModel); // sending ViewModel to view 
        }

        //to route to index method using attribute routing
        //[Route("")]
        //[Route("/Home")]          // we can delete /Home cuz we used it to route the controller
        //[Route("/Home/Index")]    // we can delete /Home cuz we used it to route the controller
        //[Route("~/")]             // /localhost/ 
        // [Route("Index")]         // /Home/Index     we can delete Index because we routed the controller
        //[Route("~/Home")]
        // index method to print all the employees 
        [AllowAnonymous]
        public ViewResult Index()
        {
            var allEmployees = _employeeRepository.getAllEmployees();
            return View(allEmployees);
        }
        [HttpGet]
        public ViewResult Create() // to get the view create.cshtml
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PhotoViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                // If the Photo property on the incoming model object is not null, then the user
                // has selected an image to upload.
                if (model.Photo != null)
                {
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));

                    //------ to apply multiple files upload
                    //if (model.Photos != null && model.Photos.Count > 0)
                    //{
                    //    // Loop thru each selected file
                    //    foreach (IFormFile photo in model.Photos)
                    //    {
                    //        // The file must be uploaded to the images folder in wwwroot
                    //        // To get the path of the wwwroot folder we are using the injected
                    //        // IHostingEnvironment service provided by ASP.NET Core
                    //        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    //        // To make sure the file name is unique we are appending a new
                    //        // GUID value and and an underscore to the file name
                    //        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    //        // Use CopyTo() method provided by IFormFile interface to
                    //        // copy the file to wwwroot/images folder
                    //        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    //    }
                    //}
                }

                Employees newEmployee = new Employees
                {
                    name = model.name,
                    email = model.email,
                    Department = model.Department,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);

                return RedirectToAction("details", new { id = newEmployee.id });
            }
            return View(); // change RedirectToActionResult to IActionResult to support both return redirectTo and return View
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employees emp = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel updatedEmployee = new EmployeeEditViewModel();
            updatedEmployee.name = emp.name;
            updatedEmployee.Department = emp.Department;
            updatedEmployee.email = emp.email;
            updatedEmployee.ExistingPhotoPath = emp.PhotoPath;

            return View(updatedEmployee);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employees emp = _employeeRepository.GetEmployee(model.Id);
                emp.name = model.name;
                emp.Department = model.Department;
                emp.email = model.email;

                string uniqueFileName = null;
                if (model.Photo != null)
                {
                    if (model.ExistingPhotoPath != null) // if there was an existing photo, when updating get this photo and delete it
                    {
                        string filePath1 = Path.Combine(hostingEnvironment.WebRootPath, "Images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath1); // to delete the file
                    }
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    emp.PhotoPath = uniqueFileName;
                    //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create)); // this line of code will make an error if we created a new employee and then edit it immediatly, give error cuz filestream proccess must be finish first
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Photo.CopyTo(fileStream);
                    }
                }
                _employeeRepository.Update(emp);
                return RedirectToAction("Index");   
            }
            return View(model);
        }
    }
}
