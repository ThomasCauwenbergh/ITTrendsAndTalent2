using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EvaluatieApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            if (User.IsInRole("Docenten"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (User.IsInRole("EvaluatieManager"))
            {
                return RedirectToAction("Index", "Evaluator");
            }
            return View();
        }

        // GET: Home/Details/5
        
    }
}
