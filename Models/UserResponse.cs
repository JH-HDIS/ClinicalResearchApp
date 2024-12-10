using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicalResearchApp.Models
{
    public class UserResponse
    {
        public string? Id { get; set; }

        // RTCCompletionDate is not required
        public DateTime? RTCCompletionDate { get; set; }

        // Only the IRBApplicationNumber is required
        [Required]
        public string? IRBApplicationNumber { get; set; }
        // All other fields are not required
        public string? PIFirstName { get; set; }
        public string? PILastName { get; set; }
        public string? PIJHED { get; set; }
        public string? PIEmailAddress { get; set; }
        public string? PrincipalInvestigator { get; set; }
        public string? Status { get; set; }
        public string? StartDate { get; set; }
        public string? StudyName { get; set; }
        public string? StudyContactFirstName { get; set; }
        public string? StudyContactLastName { get; set; }
        public string? StudyContactJHED { get; set; }
        public string? StudyContactEmailAddress { get; set; }

        // Optional fields for booleans and integers
        public bool? InvolvesSensitiveHealthInfo { get; set; }
        public int? NumberOfPeopleOrRecords { get; set; }
        public int? HumanDataSharingLevel { get; set; }
        public bool? AllActivitiesCoveredByConsent { get; set; }

         public string? Sharing_Handled_ORA_JHURA { get; set; }

         public string? Tier { get; set; }

        // List of DataClassifications (not required)
        public List<DataClassification> DataClassifications { get; set; } = new List<DataClassification>();
    }
}
