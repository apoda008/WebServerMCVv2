using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServerMVCv2.Models;
using WebServerMVCv2.Services.Cache;
using System.Linq;

namespace WebServerMCVv2.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index(HomeModel model)
        {
            model._idTitleDictionary = IdTitleDictionaryCache.GetorLoadCache();
            if (model._idTitleDictionary == null || model._idTitleDictionary.Count == 0)
            {
                Console.WriteLine("ID-Title dictionary is empty or null.");
            }
            model.SetRowCalculation();

            return View(model);
        }

        //================================
        [Authorize]
        public IActionResult UsersOnly() {
            return View();
        }
        [Authorize("admin")]
        
        public IActionResult AdminOnly() 
        {
            return View();
        }
        //================================

        [HttpGet]
        public IActionResult More(int skip = 0, int take = 30)
        {
            var idTitleDictionary = IdTitleDictionaryCache.GetorLoadCache(); // Dictionary<int,string>
            var ordered = idTitleDictionary.OrderBy(kvp => kvp.Key);
            var nextBatch = ordered.Skip(skip).Take(take).ToList();

            if (nextBatch.Count == 0)
                return Content(string.Empty); // no more results

            return PartialView("_MovieGridItems", nextBatch);
        }

        public IActionResult About() 
        {
            return View();
        }

        public ActionResult Contact() 
        {
            return View("Contact");
        }
    }
}
