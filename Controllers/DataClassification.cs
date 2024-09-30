namespace ClinicalResearchApp.Models
{
    public class DataClassification
    {
        public string? Option { get; set; } // Nullable string
        public string? Description { get; set; } // Nullable string
        public bool DataLeavingJHM { get; set; }
        public bool PHILDS { get; set; }
        public bool PHI { get; set; }
        public bool PII { get; set; }
        public bool LDS { get; set; }
        public bool PersonNoPHI { get; set; }
        public bool Counts { get; set; }

        // Preferred Data Storage Options
        public bool SAFERorSAFEDesktop { get; set; }
        public bool JHPMAP { get; set; }
        public bool JHUPhoenix { get; set; }
        public bool JHUOpenSpecimen { get; set; }
        public bool JHUQualtrics { get; set; }
        public bool JHUACHREDCap { get; set; }
        public bool SAFESTOR { get; set; }
        public bool DiscoveryHPC { get; set; }
        public bool EnterpriseNetworkStorageNAS { get; set; }
        public bool ITJHRITManagedAzureAWS { get; set; }

        // Justifiable Data Storage Options
        public bool OneDrive { get; set; }
        public bool LocalComputer { get; set; }

        // External Data Storage Tools
        public bool NonJHUREDCap { get; set; }
        public bool NonJHUSystem { get; set; }

        // Categories
        public string? ToolCategory { get; set; } // Nullable string
        public string? ResourceCategory { get; set; } // Nullable string
    }
}