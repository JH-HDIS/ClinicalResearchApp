using ClinicalResearchApp.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using Serilog;
using Microsoft.JSInterop.Infrastructure;

namespace ClinicalResearchApp.Data
{
    public class ResearchRepository
    {
        private readonly string _connectionString;

        public ResearchRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ResearchData> GetResearchData(string userJHED, string userName)
        {
            List<ResearchData> researchDataList = new();
            string piJhed = "";
            string contactJhed = "";
            string adminFlag = GetUserRole(userJHED, userName);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_slct_Risk_Tier_Calculation_ALL", conn)) // Assume the stored procedure is GetResearchData
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@UserRole", userRole);
                    
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ResearchData researchData = new()
                            {
                                Id = reader["IRB_Application_Number"].ToString(),
                                StudyName = reader["Study_Name"].ToString(),
                                PrincipalInvestigator = reader["PI_Last_Name"].ToString() + ", " + reader["PI_First_Name"].ToString(),
                                Status = reader["tier_calculator_completed_yn?"].ToString(),
                                StartDate = (DateTime)reader["RTC_Completion_Date"],
                                Tier = reader["tier"].ToString()
                                //EndDate = (DateTime)reader["EndDate"]
                            };
                            piJhed = reader["PI_JHED"].ToString();
                            contactJhed = reader["Study_Contact_JHED"].ToString();
                            researchData.AdminFlag = adminFlag;
                            if (adminFlag == "Y") { researchDataList.Add(researchData); }
                            else if (piJhed == userJHED) { researchDataList.Add(researchData); }
                            else if (contactJhed == userJHED) {researchDataList.Add(researchData); }
                            Log.Logger.Information($"In SQL select....StudyName is {researchData.StudyName}");
                
                        }
                    }
                }
            }
            return researchDataList;
        }
        
        private string GetUserRole(string userJHED, string userName)
        {
           string adminFlag = "N";
           string role = "";
           string defaultView = "Researcher";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_slct_user_role", conn)) // Assume the stored procedure is GetResearchData
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@jhed", userJHED);
                    
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                role = reader["Role"].ToString();
                                defaultView = reader["DefaultView"].ToString();
                                if (role == "Admin") { adminFlag = "Y"; }
                                else { adminFlag = "N"; }
                            }
                        }
                        else
                        {
                            reader.Close();
                            // If no rows are returned, set default values and insert a new record
                            using (SqlCommand insertCmd = new SqlCommand("usp_insert_user_role", conn))
                            {
                                insertCmd.CommandType = CommandType.StoredProcedure;
                                insertCmd.Parameters.AddWithValue("@jhed", userJHED);
                                insertCmd.Parameters.AddWithValue("@name", userName);
                                insertCmd.Parameters.AddWithValue("@Role", "Non-Admin");
                                insertCmd.Parameters.AddWithValue("@DefaultView", "" );                              
                                insertCmd.ExecuteNonQuery();
                                
                            }
                            adminFlag = "N";
                            role = "Non-ADmin";
                            defaultView = "";
                        }
                        
                            
                
                    }
                }
            }
            return adminFlag;
        }


        public List<ResearchTableData> GetResearchDataDetails(string irbNum)
        {
            List<ResearchTableData> researchTableDataList = new ();
            

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_slct_Data_Location_and_Identification", conn)) // Assume the stored procedure is GetResearchDetailsById
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IRB_Application_Number", irbNum);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                         while (reader.Read())
                        {
                            ResearchTableData researchTableData = new()
                            {
                                IRBNumber = (string)reader["IRB_Application_Number"],
                                Storage_Location = (string)reader["Storge_Location"],
                                Storage_Type = (string)reader["storage_type"],
                                Managed_by_JHIT = (string)reader["Managed_by_JHIT_yn"],
                                RequiredReview = (string)reader["RequiredReview_yn"],
                                Sharing_Handled_ORA_JHURA = (string)reader["sharing_handled_by_ora_or_jhura_ym"],
                                Text_PHI = (string)reader["Text_PHI_yn"],
                                PHI_Grtr_LDS = (string)reader["PHI_grtr_LDS_yn"],
                                LDS = (string)reader["LDS_yn"],
                                PHI_no_PII = (string)reader["PHI_no_PII_yn"],
                                PersonalData_noPHIPII = (string)reader["Prsnl_data_noPHI_orPII_yn"]
                            };
                            researchTableDataList.Add(researchTableData);
                        }
                    }
                }
            }
            return researchTableDataList;

        }

public UserResponse GetUserResponseDetails(string irbNum)
       {
            UserResponse researchData = new UserResponse();
            DataClassification dataOption = new DataClassification();
            List<DataClassification> dataClassifications = new List<DataClassification>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_slct_Risk_Tier_Calculation", conn)) // Assume the stored procedure is GetResearchDetailsById
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IRB_Application_Number", irbNum);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                           {
                            researchData = new UserResponse
                            {
                                Id = (string)reader["IRB_Application_Number"],
                                IRBApplicationNumber = reader["IRB_Application_Number"].ToString(),
                                StudyName = reader["Study_Name"].ToString(),
                                //PrincipalInvestigator = reader["PI_Last_Name"].ToString() + ", " + reader["PI_First_Name"].ToString(),
                                PIFirstName = reader["PI_First_Name"].ToString(),
                                PILastName = reader["PI_Last_Name"].ToString(),
                                //Status = reader["tier_calculator_completed_yn?"].ToString(),
                                RTCCompletionDate = (DateTime)reader["RTC_Completion_Date"],
                                //EndDate = (DateTime)reader["RTC_Completion_Date"],
                                PIJHED = reader["PI_JHED"].ToString(),
                                PIEmailAddress = reader["PI_Email_Address"].ToString(),
                                StudyContactFirstName = reader["Study_Contact_First_Name"].ToString(),
                                StudyContactLastName = reader["Study_Contact_Last_name"].ToString(),
                                StudyContactJHED = reader["Study_Contact_JHED"].ToString(),
                                StudyContactEmailAddress = reader["Study_Contact_Email_Address"].ToString(),
                                InvolvesSensitiveHealthInfo = (reader["sensitive_health_information_required_yn?"].ToString() == "Y") ? true : false,
                                //NumberOfPeopleOrRecords = reader["expected_enrollee_count"].ToString(),
                                AllActivitiesCoveredByConsent = (reader["covered_by_consent_yn?"].ToString() =="Y") ? true : false, 
                                Tier = reader["tier"].ToString()
                                    

                            };
                            if (reader["expected_enrollee_count"].ToString() == "1-499") {researchData.NumberOfPeopleOrRecords = 0;}
                            else if (reader["expected_enrollee_count"].ToString() == "500-9,999") {researchData.NumberOfPeopleOrRecords = 1;}
                            else if (reader["expected_enrollee_count"].ToString() == "> 10,000") {researchData.NumberOfPeopleOrRecords = 2;}
                        
                            if (reader["human_data_cms"].ToString() == "Directly identifiable data") {researchData.HumanDataSharingLevel = 0;}
                            else if (reader["human_data_cms"].ToString() == "LDS") {researchData.HumanDataSharingLevel = 1;}
                            else if (reader["human_data_cms"].ToString() == "Person-level data with No PHI or PII") {researchData.HumanDataSharingLevel = 2;}
                            else if (reader["human_data_cms"].ToString() == "Aggregate (counts)") {researchData.HumanDataSharingLevel= 3;}
                            else if (reader["human_data_cms"].ToString() == "Data will not be copied, moved, or shared") {researchData.HumanDataSharingLevel = 4;}

                            reader.Close();
                            using (SqlCommand innerCmd = new SqlCommand("usp_slct_Data_Location_and_Identification", conn)) // Assume the stored procedure is GetResearchDetailsById
                                {
                                    innerCmd.CommandType = CommandType.StoredProcedure;
                                    innerCmd.Parameters.AddWithValue("@IRB_Application_Number", irbNum);

                                    using (SqlDataReader innerReader = innerCmd.ExecuteReader())
                                    {
                                        
                                        while (innerReader.Read())
                                        {
                                             dataOption = new DataClassification  {
                                                Option = (string)innerReader["Storage_Location"]
                                             };
                                             if (dataOption.Option == "SAFERorSAFEDesktop") { dataOption.Option = "2.P.1"; }
                                             if (dataOption.Option == "JHPMAP") { dataOption.Option = "2.P.2"; }
                                             if (dataOption.Option == "JHUOpenSpecimen") { dataOption.Option = "2.P.3"; }
                                             if (dataOption.Option == "JHUQualtrics") { dataOption.Option = "2.P.4"; }
                                             if (dataOption.Option == "JHUACHREDCap") { dataOption.Option = "2.P.5"; }
                                            if (dataOption.Option == "SAFESTOR") { dataOption.Option = "2.P.6"; }
                                            if (dataOption.Option == "DiscoveryHPC") { dataOption.Option = "2.P.7"; }
                                            if (dataOption.Option == "EnterpriseNetworkStorageNAS") { dataOption.Option = "2.P.8"; }
                                            if (dataOption.Option == "ITJHRITManagedAzureAWS") { dataOption.Option = "2.P.9"; }
                                            if (dataOption.Option == "OneDrive") { dataOption.Option = "2.J.1"; }
                                            if (dataOption.Option == "LocalComputer") { dataOption.Option = "2.J.2"; }
                                            if (dataOption.Option == "NonJHU_REDCap") { dataOption.Option = "2.E.1"; }
                                            if (dataOption.Option == "NonJHUSystem") { dataOption.Option = "2.E.2"; }
                                            if (dataOption.Option == "DepartmentServer") { dataOption.Option = "2.R.1"; }
                                            if (dataOption.Option == "OtherComputers") { dataOption.Option = "2.R.2"; }
                                            if (dataOption.Option == "USB") { dataOption.Option = "2.R.3"; }
                                            if (dataOption.Option == "JHPCE") { dataOption.Option = "2.R.4"; }
                                            if (dataOption.Option == "JHUARCH") { dataOption.Option = "2.R.5"; }
                                            if (dataOption.Option == "OtherSolutions") { dataOption.Option = "2.R.6"; }


                                             
                                             dataOption.Column = "C7";
                                            string dataloc = (string)innerReader["Text_PHI_yn"];
                                            if (dataloc == "Y") {
                                                    dataOption.Column = "C1";
                                                    dataOption.Selected = true;}
                
                                            dataloc = (string)innerReader["PHI_grtr_LDS_yn"];
                                            if (dataloc == "Y") {
                                                    dataOption.Column = "C2";
                                                    dataOption.Selected = true;}
                                            dataloc = (string)innerReader["LDS_yn"];
                                            if (dataloc == "Y") {
                                                dataOption.Column = "C3";
                                                dataOption.Selected = true;}
                                             dataloc = (string)innerReader["PHI_no_PII_yn"];
                                            if (dataloc == "Y") {
                                                dataOption.Column = "C4";
                                                dataOption.Selected = true;}
                                            dataloc = (string)innerReader["Prsnl_data_noPHI_orPII_yn"];
                                            if (dataloc == "Y") {
                                                dataOption.Column = "C5";
                                                dataOption.Selected = true;}
                                            dataloc = (string)innerReader["Aggregate_Counts_yn"];
                                            if (dataloc == "Y") {
                                                dataOption.Column = "C6";
                                                dataOption.Selected = true;}
                                            dataClassifications.Add(dataOption);
                                        
                                         researchData.Sharing_Handled_ORA_JHURA =  (innerReader["sharing_handled_by_ora_or_jhura_yn?"].ToString() == "Y") ? "Yes" : "No";    
                                        }
                                                
                                    }
                                }
                            researchData.DataClassifications = dataClassifications;
                        }
                                
                    }
                }
            }
            Log.Logger.Information($"GetUserReponseDetails....");

            for (int i = 0; i < researchData.DataClassifications.Count; i++)
                {
                    Log.Logger.Information($"GetUserReponseDetails..DataClassificationsOption..{researchData.DataClassifications[i].Option}");
                    Log.Logger.Information($"GetUserReponseDetails..DataClassificationsColumn..{researchData.DataClassifications[i].Column}");
                    Log.Logger.Information($"GetUserReponseDetails..DataClassificationsSelected..{researchData.DataClassifications[i].Selected}"); 
                }
           
           /*
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_slct_Data_Location_and_Identification", conn)) // Assume the stored procedure is GetResearchDetailsById
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IRB_Application_Number", irbNum);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                           {
                            string dataloc = (string)reader["Storage_Location"];
                            {
                                if (dataloc == "JH_PMAP") {researchData.DataClassifications[0].JHPMAP = true;}
                            }
                           
                        }
                                
                    }
                }
           
            }
            */
            return researchData ?? new UserResponse();
        }

        public void UpdateResearchData(ResearchData data)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_upsert_Risk_Tier_Calculation", conn)) // Assume the stored procedure is UpdateResearchData
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IRB_Application_Number", data.Id);
                    cmd.Parameters.AddWithValue("@PI_First_Name", data.PI_First_Name);
                    cmd.Parameters.AddWithValue("@PI_Last_Name", data.PI_Last_Name);
                    cmd.Parameters.AddWithValue("@PI_JHED", data.PI_JHED);
                    cmd.Parameters.AddWithValue("@PI_Email_Address", data.PI_Email_Address);
                    cmd.Parameters.AddWithValue("@Study_Contact_First_Name", data.Study_Contact_First_Name);
                    cmd.Parameters.AddWithValue("@Study_Contact_JHED", data.Study_Contact_JHED);
                    cmd.Parameters.AddWithValue("@Study_Contact_Last_Name", data.Study_Contact_Last_name);
                    cmd.Parameters.AddWithValue("@Study_contact_Email_Address", data.Study_Contact_Email_Address);
                    cmd.Parameters.AddWithValue("@sensitive_health_information_required_yn", data.Sensitive_Health_Info);
                    cmd.Parameters.AddWithValue("@expected_enrollee_count", data.Expected_Enroll_Count);
                    cmd.Parameters.AddWithValue("@covered_by_consent_yn", data.Covered_By_Consent);
                    cmd.Parameters.AddWithValue("@tier_calculator_completed_yn", "Y");
                    cmd.Parameters.AddWithValue("@humanSharingLevel", data.Human_data_cms);    
                    cmd.Parameters.AddWithValue("@tier", data.Tier);
                    cmd.Parameters.AddWithValue("@Study_Name", data.StudyName);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

         public void UpdateDataStorage(string irbNum, string converedByConsent, string handledByORA, List<DataClassification> data)
        {
            Log.Logger.Information("In the UpdateDataStorage function");
            for (int i = 0; i < data.Count; i++)
                    {
                        Log.Logger.Information($"The data option is: {data[i].Option}");
                        Log.Logger.Information($"The data column is: {data[i].Column}");
                        Log.Logger.Information($"The data selected is: {data[i].Selected}");
                    }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("usp_upsert_Data_Location_and_Identification", conn)) // Assume the stored procedure is UpdateResearchData
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    for (int i = 0; i < data.Count; i++)
                    {
                        cmd.Parameters.Clear();
                        var dataLoc = data[i];
                        if (dataLoc.Column =="C1")
                            {
                                cmd.Parameters.AddWithValue("@Text_PHI_yn", "Y");
                            }
                        else {
                                cmd.Parameters.AddWithValue("@Text_PHI_yn", "N");
                            }
                        if (dataLoc.Column =="C2") 
                            {
                                cmd.Parameters.AddWithValue("@PHI_grtr_LDS_yn", "Y");
                            }
                        else { 
                                cmd.Parameters.AddWithValue("@PHI_grtr_LDS_yn", "N"); 
                            }
                        if (dataLoc.Column =="C3")
                        {
                            cmd.Parameters.AddWithValue("@LDS_yn", "Y");
                        }
                        else {
                            cmd.Parameters.AddWithValue("@LDS_yn", "N");
                        }
                        if (dataLoc.Column =="C4") 
                        {
                            cmd.Parameters.AddWithValue("@PHI_no_PII_yn", "Y");
                        }
                        else {
                            cmd.Parameters.AddWithValue("@PHI_no_PII_yn", "N");                 
                        }
                        if (dataLoc.Column =="C5")
                        {
                            cmd.Parameters.AddWithValue("@Prsnl_data_noPHI_orPII_yn", "Y");
                        }
                        else {
                            cmd.Parameters.AddWithValue("@Prsnl_data_noPHI_orPII_yn", "N"); 
                        }
                        if (dataLoc.Column =="C6")
                        {
                            cmd.Parameters.AddWithValue("@Aggregate_Counts_yn", "Y");
                        }
                        else {
                            cmd.Parameters.AddWithValue("@Aggregate_Counts_yn", "N");
                        }
                        if (dataLoc.Column =="C7")
                        {
                            cmd.Parameters.AddWithValue("@action", "delete");
                        } else {
                            cmd.Parameters.AddWithValue("@action", "merge");
                        }
                              
                        cmd.Parameters.AddWithValue("@IRB_Application_Number", irbNum);
                        cmd.Parameters.AddWithValue("@Storage_Location", dataLoc.Option);
                        cmd.Parameters.AddWithValue("@Managed_by_JHIT_yn", "N");
                        cmd.Parameters.AddWithValue("@RequiredReview_yn", "Y");
                        cmd.Parameters.AddWithValue("@storage_type", "Internal");
                        cmd.Parameters.AddWithValue("@sharing_handled_by_ora_or_jhura_yn", handledByORA);
                        cmd.Parameters.AddWithValue("@not_used_yn", "N");   
                        cmd.Parameters.AddWithValue("@tier_calculator_completed_yn", "Y");   
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
