using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicalResearchApp.Models
{
    public class UserResponse
    {
        public string? Id { get; set; }

        // RTCCompletionDate is not required
        // Note: Pre-populated with DateTime.Today (e.g., 2025-03-27) in ResearchController.Edit for new IRBs
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

        // Optional fields for booleans and integers - Required fields
        [Required(ErrorMessage = "Please answer question 1A.")]
        public bool? InvolvesSensitiveHealthInfo { get; set; }

        [Required(ErrorMessage = "Please answer question 1B.")]
        public int? NumberOfPeopleOrRecords { get; set; }

        [Required(ErrorMessage = "Please answer question 1C.")]
        public int? HumanDataSharingLevel { get; set; }

        [Required(ErrorMessage = "Please answer question 1D.")]
        public bool? AllActivitiesCoveredByConsent { get; set; }

        public string? Sharing_Handled_ORA_JHURA { get; set; }

        public string? Tier { get; set; }

        // List of DataClassifications (not required)
        public List<DataClassification> DataClassifications { get; set; } = new List<DataClassification>();
    }
}