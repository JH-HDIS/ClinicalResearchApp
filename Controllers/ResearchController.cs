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
    public IActionResult Index(
    string role, 
    string sortOrder, 
    string irbFilter, 
    string studyFilter,
    string investigatorFilter, 
    string statusFilter, 
    string dateFilter, 
    string tierFilter)
            {

                // Retrieve the username and jhed value
                string userName = User.Identity.IsAuthenticated 
                    ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value 
                    : "Guest";
                ViewBag.Username = userName;
                ViewBag.jhed = User.Identity.IsAuthenticated 
                    ? User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value 
                    : "Guest";

                // Handle sorting
                ViewData["CurrentSort"] = sortOrder; // Track the current sort column
                ViewData["IRBNumberSort"] = sortOrder == "irb" ? "irb_desc" : "irb";
                ViewData["StudyNameSort"] = sortOrder == "study" ? "study_desc" : "study";
                ViewData["InvestigatorSort"] = sortOrder == "investigator" ? "investigator_desc" : "investigator";
                ViewData["StatusSort"] = sortOrder == "status" ? "status_desc" : "status";
                ViewData["UpdatedSort"] = sortOrder == "updated" ? "updated_desc" : "updated";
                ViewData["TierSort"] = sortOrder == "tier" ? "tier_desc" : "tier";

                // Preserve filter values for the view
                ViewData["IRBFilter"] = irbFilter;
                ViewData["StudyFilter"] = studyFilter;
                ViewData["InvestigatorFilter"] = investigatorFilter;
                ViewData["StatusFilter"] = statusFilter;
                ViewData["DateFilter"] = dateFilter;
                ViewData["TierFilter"] = tierFilter;

                // Get data from repository
                IEnumerable<ResearchData> data = _repository.GetResearchData(ViewBag.jhed);
                
                 Log.Logger.Information($"In RESEARCHCONTROLLER Index..");
                 foreach (var item in data) {
                        Log.Logger.Information($"The IRB Number is: {item.Id}");
                        Log.Logger.Information($"The Study Name is: {item.StudyName}");
                        Log.Logger.Information($"The Principal Investigator is: {item.PrincipalInvestigator}");
                        Log.Logger.Information($"The Status is: {item.Status}");
                        Log.Logger.Information($"The Start Date is: {item.StartDate}");
                        Log.Logger.Information($"The Tier is: {item.Tier}");
                 }
                // Apply filters
                if (!string.IsNullOrEmpty(irbFilter))
                {
                    data = data.Where(r => r.Id != null && r.Id.Contains(irbFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                 if (!string.IsNullOrEmpty(studyFilter))
                {
                    data = data.Where(r => r.StudyName != null && r.StudyName.Contains(studyFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (!string.IsNullOrEmpty(investigatorFilter))
                {
                    data = data.Where(r => r.PrincipalInvestigator != null && r.PrincipalInvestigator.Contains(investigatorFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    data = data.Where(r => r.Status != null && r.Status.Contains(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var parsedDate))
                {
                    data = data.Where(r => r.StartDate != null && r.StartDate.Date == parsedDate.Date).ToList();
                }
                if (!string.IsNullOrEmpty(tierFilter))
                {
                    data = data.Where(r => r.Tier != null && r.Tier.Contains(tierFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Apply sorting
                data = sortOrder switch
                {
                    "irb" => data.OrderBy(r => r.Id).ToList(),
                    "irb_desc" => data.OrderByDescending(r => r.Id).ToList(),
                     "study" => data.OrderBy(r => r.StudyName).ToList(),
                    "study_desc" => data.OrderByDescending(r => r.StudyName).ToList(),
                    "investigator" => data.OrderBy(r => r.PrincipalInvestigator).ToList(),
                    "investigator_desc" => data.OrderByDescending(r => r.PrincipalInvestigator).ToList(),
                    "status" => data.OrderBy(r => r.Status).ToList(),
                    "status_desc" => data.OrderByDescending(r => r.Status).ToList(),
                    "updated" => data.OrderBy(r => r.StartDate).ToList(),
                    "updated_desc" => data.OrderByDescending(r => r.StartDate).ToList(),
                    "tier" => data.OrderBy(r => r.Tier).ToList(),
                    "tier_desc" => data.OrderByDescending(r => r.Tier).ToList(),
                    _ => data.OrderBy(r => r.Id).ToList() // Default sort
                };

            
                string adminFlag = data.FirstOrDefault()?.AdminFlag.ToString();
                if (adminFlag == "Y") {
                    ViewBag.Admin = "Admin";
                } else {
                    ViewBag.Admin = "Non-Admin";
                }
                
                // Return the view with the sorted and filtered data
                return View(data);
}

        [HttpPost]
        public IActionResult Edit(string id, bool viewOnly, string createNewFlag)
        {
            //var data = _repository.GetResearchData("Normal").FirstOrDefault(x => x.Id == id);
           // var data = _repository.GetResearchDataDetails(id);
    
            var data = _repository.GetUserResponseDetails(id);
             if (viewOnly)
            {
                ViewBag.ViewOnly = true;
            }
            if (createNewFlag == "Y")
            {
                data.Id = id;
            }
            return View(data);
        }


        [HttpPost]
        public IActionResult Update(ResearchData researchData)
        {
            _repository.UpdateResearchData(researchData);
            return RedirectToAction("Index", new { role = "Normal" });
        }

        [HttpPost]
        public IActionResult SearchByIRB(string irbSearch)
        {

            // Retrieve the username and jhed value
                string userName = User.Identity.IsAuthenticated 
                    ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value 
                    : "Guest";
                ViewBag.Username = userName;
                ViewBag.jhed = User.Identity.IsAuthenticated 
                    ? User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value 
                    : "Guest";
            if (string.IsNullOrWhiteSpace(irbSearch))
            {
                ViewBag.SearchPerformed = false;
                ViewBag.Admin = "Non-Admin";
                return View("Index");
            }

            // Example logic to check if the IRB number exists
            bool irbFound = CheckIfIrbExists(irbSearch);

            ViewBag.SearchPerformed = true;
            ViewBag.IrbFound = irbFound;
            ViewBag.IrbNumber = irbSearch;
            ViewBag.Admin = "Non-Admin";

            return View("Index");
        }

        private bool CheckIfIrbExists(string irbSearch)
        {
            // Replace with your actual logic to verify the IRB number
            var data = _repository.GetUserResponseDetails(irbSearch);
            var irbFound = data.Id != null;
            if (data.Id == null)
            {
                irbFound = false;
            } else {
                irbFound = true;
            }
            Log.Logger.Information($"In CheckIfIrbExists.....data.Id is: {data.Id}");
            Log.Logger.Information($"In CheckIfIrbExists.....irbFound is: {irbFound}");
            return irbFound; // Example: assume IRB "12345" exists
        }

        [HttpPost]
        public IActionResult SaveDataClassifications(IFormCollection form)
        {
            var updatedDataClassifications = new List<DataClassification>();
            var researchData = new ResearchData();
            
            // Validate required fields using a flag
            bool hasValidationErrors = false;
            string validationErrorMessage = "";

    // Check for required fields (1A-1D)
    if (!form.ContainsKey("InvolvesSensitiveHealthInfo") || string.IsNullOrEmpty(form["InvolvesSensitiveHealthInfo"]))
    {
        hasValidationErrors = true;
        validationErrorMessage += "1A: Please answer whether sensitive health info is involved.\n";
    }
    if (!form.ContainsKey("NumberOfPeopleOrRecords") || string.IsNullOrEmpty(form["NumberOfPeopleOrRecords"]))
    {
        hasValidationErrors = true;
        validationErrorMessage += "1B: Please specify the number of people or records.\n";
    }
    if (!form.ContainsKey("HumanDataSharingLevel") || string.IsNullOrEmpty(form["HumanDataSharingLevel"]))
    {
        hasValidationErrors = true;
        validationErrorMessage += "1C: Please select a human data sharing level.\n";
    }
    if (!form.ContainsKey("AllActivitiesCoveredByConsent") || string.IsNullOrEmpty(form["AllActivitiesCoveredByConsent"]))
    {
        hasValidationErrors = true;
        validationErrorMessage += "1D: Please confirm whether all activities are covered by consent.\n";
    }

    // If validation fails, log errors and return to the form
    if (hasValidationErrors)
    {
        Log.Logger.Error($"Validation Errors: {validationErrorMessage}");
        TempData["ValidationErrors"] = validationErrorMessage;

        // Redirect back to the form with validation errors
        return RedirectToAction("Edit", new { id = form["IRBApplicationNumber"], viewOnly = false });
    }
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
                    researchData.Tier = form["riskLevel"];

                    
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
