using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NetCore_Model_View_Cortol_Created.Controllers
{

    
    public class ErrorHandeling : Controller
    {
        private readonly ILogger<ErrorHandeling> logger;

        //inject ilogger service using constructor
        public ErrorHandeling(ILogger<ErrorHandeling> logger) // we log errors from ErrorHandling controller so ILogger<ErrorHandling> is used
        {
            this.logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult ErrorHandeling404(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "This resource is invalid!";
                    logger.LogWarning($"Path of error {statusCodeResult.OriginalPath}     " +
                        $"Query = {statusCodeResult.OriginalQueryString}"); // u have to use $ dollar sign to use variable inside string value in the run time
                   
                    break;
                    //ViewBag.ErrorMessage = "This resource is invalid!";
                    //ViewBag.Url = statusCodeResult.OriginalPath;
                    //ViewBag.Parameters = statusCodeResult.OriginalQueryString;
                    //break;
            }
            return View("Error");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error(int statusCode)
        {
            // retrieve exception details
            var statusCodeResult = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            logger.LogError($"ErrorPath {statusCodeResult.Path} " +
                $"  threw an error {statusCodeResult.Error.Message} with stackTrace = {statusCodeResult.Error.StackTrace}");
            //ViewBag.ErrorPath = statusCodeResult.Path;
            //ViewBag.ErrorMessage = statusCodeResult.Error.Message;
            //ViewBag.StackTrace = statusCodeResult.Error.StackTrace;


            return View("Error1");
        }
    }
}
