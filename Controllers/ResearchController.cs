using ClinicalResearchApp.Data;
using ClinicalResearchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

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

        [Authorize]
        public IActionResult Index(string role)
        {
            var data = _repository.GetResearchData(role);
            string userName = User.Identity.IsAuthenticated? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value: "Guest";
            ViewBag.Username = userName;
            ViewBag.jhed = User.Identity.IsAuthenticated? User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value: "Guest";
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


        [HttpPost]
        public IActionResult SaveDataClassifications(IFormCollection form)
        {
            var updatedDataClassifications = new List<DataClassification>();
            var researchData = new ResearchData();
            var riskTier = "";

            // Loop through form keys to find selected options
            foreach (var key in form.Keys)
            {
                Log.Logger.Information($"In SAVEDATACLASSIFICATIONS.....The key is: {key}");
                // Example: Key format should be "SAFERorSAFEDesktop" or "JHPMAP"
                if (key.StartsWith("SAFERorSAFEDesktop") || key.StartsWith("JHPMAP") || key.StartsWith("JHUOpenSpecimen") || 
                    key.StartsWith("JHUQualtrics" )|| key.StartsWith("JHUACHREDCap") || key.StartsWith("SAFESTOR") ||
                    key.StartsWith("DiscoveryHPC") || key.StartsWith("EnterpriseNetworkStorageNAS") || key.StartsWith("ITJHRITManagedAzureAWS") ||
                    key.StartsWith("OneDrive") || key.StartsWith("LocalComputer") || key.StartsWith("NonJHU_REDCap") ||
                    key.StartsWith("NonJHUSystem"))
                    {
                        var selectedColumn = form[key]; // Value of the selected radio button
                        var option = key; // Option name (e.g., "SAFERorSAFEDesktop" or "JHPMAP")
                        //updatedDataClassifications.RemoveAll(dc => dc.Column == "C7");
                        //if (selectedColumn != "C7") {
                            // Map to your DataClassification model
                            updatedDataClassifications.Add(new DataClassification
                            {
                                Option = option,
                                Column = selectedColumn,
                                Selected = true // This is the selected radio button
                            });
                        //} 
                    }
                else {
                    researchData.Id = form["IRBApplicationNumber"];
                    researchData.StudyName = form["StudyName"];
                    researchData.PrincipalInvestigator = form["PrincipalInvestigator"];
                    researchData.PI_First_Name = form["PIFirstName"];
                    researchData.PI_Last_Name = form["PILastName"];
                    //researchData.Status = form["Status"];
                    //researchData.StartDate = Convert.ToDateTime(form["StartDate"]);
                    //researchData.EndDate = Convert.ToDateTime(form["EndDate"]);
                    researchData.PI_JHED = form["PIJHED"];
                    researchData.PI_Email_Address = form["PIEmailAddress"];
                    researchData.Study_Contact_First_Name = form["StudyContactFirstName"];
                    researchData.Study_Contact_Last_name = form["StudyContactLastName"];
                    researchData.Study_Contact_JHED = form["StudyContactJHED"];
                    researchData.Study_Contact_Email_Address = form["StudyContactEmailAddress"];
                    //researchData.Sensitive_Health_Info = form["InvolvesSensitiveHealthInfo"];
                    researchData.Sensitive_Health_Info = form["InvolvesSensitiveHealthInfo"] == "true" ? "Y" : "N";
                    //researchData.Expected_Enroll_Count = form["NumberOfPeopleOrRecords"];
                    switch (form["NumberOfPeopleOrRecords"])
                    {
                        case "0":
                            researchData.Expected_Enroll_Count = "1-499";
                            break;
                        case "1":
                            researchData.Expected_Enroll_Count = "500-9,999";
                            break;
                        case "2":
                            researchData.Expected_Enroll_Count = "> 10,000";
                            break;
                        default:
                            researchData.Expected_Enroll_Count = "Unknown"; // Optional default value
                            break;
                    }
                    //researchData.Human_data_cms = form["HumanDataSharingLevel"];
                    switch (form["HumanDataSharingLevel"])
                    {
                        case "0":
                            researchData.Human_data_cms = "Directly identifiable data";
                            break;
                        case "1":
                            researchData.Human_data_cms = "LDS";
                            break;
                        case "2":
                            researchData.Human_data_cms = "Person-level data with No PHI or PII";
                            break;
                        case "3":    
                            researchData.Human_data_cms = "Aggregate (counts)";
                            break;  
                        case "4":        
                            researchData.Human_data_cms = "Data will not be copied, moved, or shared";
                            break;
                        default:
                            researchData.Human_data_cms = "Unknown"; // Optional default value
                            break;
                    }
                    researchData.Covered_By_Consent = form["AllActivitiesCoveredByConsent"] == "true" ? "Y" : "N";
                    researchData.Sharing_Handled_ORA_JHURA = form["DataSharingAgreement"] == "Yes" ? "Y" : "N";

                    //riskTier = form["risk-level-note"];
                    //researchData.tier =  riskTier.Substring(riskTier.Length - 6);

                    
                }
            }

            // Call a method to save to the database
            _repository.UpdateResearchData(researchData);
           Log.Logger.Information("In the SaveDataClassifications function");
            Log.Logger.Information("After the UpdateResearchData funtion and Before the UpdateDataStorage function");
            for (int i = 0; i < updatedDataClassifications.Count; i++)
                    {
                        Log.Logger.Information($"The data option is: {updatedDataClassifications[i].Option}");
                        Log.Logger.Information($"The data column is: {updatedDataClassifications[i].Column}");
                        Log.Logger.Information($"The data selected is: {updatedDataClassifications[i].Selected}");
                    }
            _repository.UpdateDataStorage(researchData.Id,researchData.Covered_By_Consent,researchData.Sharing_Handled_ORA_JHURA,updatedDataClassifications);


            // Redirect or return a view
            return RedirectToAction("Index"); // Or wherever you want to navigate after saving
        }
    }
}
