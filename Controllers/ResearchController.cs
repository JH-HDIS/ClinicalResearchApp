using ClinicalResearchApp.Data;
using ClinicalResearchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ClinicalResearchApp.Controllers
{
    public class ResearchController : Controller
    {
        private readonly ResearchRepository _repository;

        public ResearchController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            }
            _repository = new ResearchRepository(connectionString);
        }

        public IActionResult Index(string role)
        {
            var data = _repository.GetResearchData(role);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(string id)
        {
            //var data = _repository.GetResearchData("Normal").FirstOrDefault(x => x.Id == id);
           // var data = _repository.GetResearchDataDetails(id);
            var data = _repository.GetUserResponseDetails(id);
            return View(data);
        }

        // This action returns the dynamic message as plain text
        public JsonResult GetDynamicMessage()
        {
            string message = "This is a dynamic message that appears on scroll!";
            return Json(message);
        }


        [HttpPost]
        public IActionResult Update(ResearchData researchData)
        {
            _repository.UpdateResearchData(researchData);
            return RedirectToAction("Index", new { role = "Normal" });
        }
    }
}
