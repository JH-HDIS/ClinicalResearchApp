using Microsoft.Identity.Client;

namespace ClinicalResearchApp.Models
{
    public class ResearchData
    {
        public string? Id { get; set; }
        public string? StudyName { get; set; }
        public string? PrincipalInvestigator { get; set; }
        public string? PI_First_Name { get; set; }
        public string? PI_Last_Name { get; set; }

        public string? Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? PI_JHED {get; set;}
        public string? PI_Email_Address {get; set;}
        public string? Study_Contact_First_Name {get; set;}
        public string? Study_Contact_Last_name {get; set;}
        public string? Study_Contact_JHED {get; set;}

        public string? Study_Contact_Email_Address {get; set;}
        public string? Sensitive_Health_Info {get; set;}    

        public string? Expected_Enroll_Count {get; set;}

        public string? Covered_By_Consent {get; set;}

    }
}
