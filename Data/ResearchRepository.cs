using ClinicalResearchApp.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace ClinicalResearchApp.Data
{
    public class ResearchRepository
    {
        private readonly string _connectionString;

        public ResearchRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ResearchData> GetResearchData(string userRole)
        {
            List<ResearchData> researchDataList = new();

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
                                StudyName = reader["IRB_Application_Number"].ToString() + " Study Name",
                                PrincipalInvestigator = reader["PI_Last_Name"].ToString() + ", " + reader["PI_First_Name"].ToString(),
                                Status = reader["tier_calculator_completed_yn?"].ToString(),
                                StartDate = (DateTime)reader["RTC_Completion_Date"],
                                //EndDate = (DateTime)reader["EndDate"]
                            };
                            researchDataList.Add(researchData);
                        }
                    }
                }
            }
            return researchDataList;
        }

        public ResearchData GetResearchDataDetails(string irbNum)
        {
            ResearchData researchData = new ResearchData();

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
                            researchData = new ResearchData
                            {
                                Id = (string)reader["IRB_Application_Number"],
                                StudyName = reader["IRB_Application_Number"].ToString() + " Study Name",
                                PrincipalInvestigator = reader["PI_Last_Name"].ToString() + ", " + reader["PI_First_Name"].ToString(),
                                PI_First_Name = reader["PI_First_Name"].ToString(),
                                PI_Last_Name = reader["PI_Last_Name"].ToString(),
                                Status = reader["tier_calculator_completed_yn?"].ToString(),
                                StartDate = (DateTime)reader["RTC_Completion_Date"],
                                EndDate = (DateTime)reader["RTC_Completion_Date"],
                                PI_JHED = reader["PI_JHED"].ToString(),
                                PI_Email_Address = reader["PI_Email_Address"].ToString(),
                                Study_Contact_First_Name = reader["Study_Contact_First_Name"].ToString(),
                                Study_Contact_Last_name = reader["Study_Contact_Last_name"].ToString(),
                                Study_Contact_JHED = reader["Study_Contact_JHED"].ToString(),
                                Study_Contact_Email_Address = reader["Study_Contact_Email_Address"].ToString(),
                                Sensitive_Health_Info = reader["sensitive_health_information_required_yn?"].ToString(),
                                Expected_Enroll_Count = reader["expected_enrollee_count"].ToString(),
                                Covered_By_Consent = reader["covered_by_consent_yn?"].ToString()

                            };
                        }
                    }
                }
            }
            return researchData ?? new ResearchData();

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
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
