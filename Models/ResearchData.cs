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

        public string? Human_data_cms {get; set;}
        public string? Covered_By_Consent {get; set;}

        public string? Sharing_Handled_ORA_JHURA { get; set; }
        public string? Tier { get; set; }

        public string? AdminFlag { get; set; }
    }

    public class ResearchTableData {
        public string? IRBNumber { get; set; } 
        public string? Storage_Location { get; set; }
        public string? Storage_Type { get; set; }  
        public string? Managed_by_JHIT { get; set; }
        public string? RequiredReview { get; set; }  
        public string? Sharing_Handled_ORA_JHURA { get; set; }
        public string? Text_PHI { get; set; }
        public string? PHI_Grtr_LDS { get; set; }
        public string? LDS { get; set; }
        public string? PHI_no_PII { get; set; }     
        public string? PersonalData_noPHIPII { get; set; }
        public string? Tier { get; set; }

    }
}
