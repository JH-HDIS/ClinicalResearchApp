using ClinicalResearchApp.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using Serilog;
using System.Runtime.CompilerServices;

namespace ClinicalResearchApp.Data
{
     public class Utility
    {
        
        public Utility()
        {
        }

        public static string CalculateTier(string[,] array, string dataLeavingJHM, string numRecords, string consentYN, string humanDataSharingLevel, string involvesSensitiveHealthInfo, string dataSharingAgreement)
        {
            
            string toolType = "";
            string rowId = "";
            string colPos = "";
            string dataLeaving = dataLeavingJHM;
            string numRec = numRecords;
            string consent = consentYN;
            string hDataSharingLevel = humanDataSharingLevel;
            string involvesSensitiveHealth = involvesSensitiveHealthInfo;
            string dataSharing = dataSharingAgreement;
            if (hDataSharingLevel == "4") // 4 is Data will not be copied, moved, or shared
            {
                dataLeaving = "N";
            } else {
                dataLeaving = "Y";
            }
            Log.Logger.Information("In the CalculateTier function");
            Log.Logger.Information($"The dataLeavingJHM is: {dataLeaving}"); 
            Log.Logger.Information($"The involvesSensitiveHealthInfo is: {involvesSensitiveHealth}");
            Log.Logger.Information($"The numRecords is: {numRec}");
            Log.Logger.Information($"The consentYN is: {consent}");
            Log.Logger.Information($"The humanDataSharingLevel is: {hDataSharingLevel}");   
            Log.Logger.Information($"The dataSharing is: {dataSharing}");   


            List<string> tiers = new List<string>();

            int arySize = array.GetLength(0);
            // Initialize the variable to track the lowest score.
            // Start with a high value to ensure any actual row sum will be lower.
            String lowestTier = "";

            // Loop over each row
            for (int i = 0; i < arySize; i++)
            {
                
                // Loop over each element in the row
                for (int j = 0; j < 2; j++)
                {
                    if (j == 0) {
                        toolType = array[i, j].Substring(2,1); // Add the element to the row sum
                        rowId = array[i, j];
                        Log.Logger.Information($"The toolType is: {toolType}");
                    }
                    if (j == 1)
                    {
                        colPos = array[i, j]; // Add the element to the row sum
                        Log.Logger.Information($"The colPos is: {colPos}");

                        // Calculate Tier for the row
                        if ((toolType == "P") && (colPos != "C7"))
                        {
                            if ((hDataSharingLevel == "4") && (consent == "Y")) // 4 is Data will not be copied, moved, or shared
                            {
                                tiers.Add("Tier A");
                            }

                            else
                            {
                                if ((colPos == "C1" || colPos == "C2") && (hDataSharingLevel != "4"))// 1 is Text PHI, 2 is PHI > LDS
                                {
                                    tiers.Add("Tier C");
                                }
                                else
                                {
                                    if (numRec == "2") // 2 is 10,000 or more
                                    {
                                        tiers.Add("Tier C");
                                    }
                                    else
                                    {
                                        tiers.Add("Tier B");
                                    }
                                }
                            }
                        }


                        if ((toolType == "J") && (colPos != "C7"))
                        {
                            Log.Logger.Information($"In toolType J, colPos: {colPos}");
                            Log.Logger.Information($"In toolType J, involvesSensitiveHealth: {involvesSensitiveHealth}");
                            Log.Logger.Information($"In toolType J, consent: {consent}");

                            if ((colPos == "C6" || colPos == "C5") && involvesSensitiveHealth == "N")
                            {
                                Log.Logger.Information($"In toolType J, adding Tier A");
                                tiers.Add("Tier A");
                            }

                            else
                            {
                                if (consent == "Y")
                                {
                                    tiers.Add("Tier A");
                                }
                                else
                                {
                                    if (colPos == "C3" || colPos == "C4" || colPos == "C5" || colPos == "C6")
                                    {
                                        tiers.Add("Tier B");
                                    }
                                    else   // 1 is Text PHI, 2 is PHI > LDS
                                    {
                                        if (numRec == "1" || numRec == "2") // 2 is 10,000 or more
                                        {
                                            tiers.Add("Tier C");
                                        }
                                        else
                                        {
                                            if (dataLeaving == "N")
                                            {
                                                tiers.Add("Tier B");
                                            }
                                            else
                                            {
                                                tiers.Add("Tier C");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if ((toolType == "R") && (colPos != "C7"))
                        {
                            if ((colPos == "C3" || colPos == "C4" || colPos == "C5" || colPos == "C6") && involvesSensitiveHealth == "N")
                            {
                                tiers.Add("Tier B");
                            }
                            else
                            {
                                tiers.Add("Tier C");
                            }
                        }
                        if ((toolType == "E") && (colPos != "C7"))
                        {
                            Log.Logger.Information($"ToolType is E");
                            Log.Logger.Information($"The dataSharing is: {dataSharing}");
                            if (dataSharing == "Y")
                            {
                                // No change if covered by consent agreement
                                //tiers.Add("Tier C");
                                Log.Logger.Information($"No change Tool E & covered by consent agreement.");
                            }
                            else
                            {
                                tiers.Add("Tier C");
                                Log.Logger.Information($"Adding Tier C to the list");
                            }

                        }
                        // other conditions
                        if ((colPos == "C1" || colPos == "C2" || colPos == "C3" || colPos == "C4") && (rowId == "2.R.2"))
                        {
                            tiers.Add("Tier X");
                        }

                    }
                }
            } // End of loop over each row


            // Loop over each row sum to find the lowest score
            for (int i = 0; i < tiers.Count; i++)
                {
                    Log.Logger.Information($"Tiers[i]: {tiers[i]}");
                   Log.Logger.Information($"Lowest Tier: {lowestTier}");
                     
                    if (tiers[i].StartsWith("Tier X") && (lowestTier.StartsWith("Tier C") || lowestTier.StartsWith("Tier B") || lowestTier.StartsWith("Tier A") || lowestTier == "")) {
                        lowestTier = "Tier X - Prohibited; revise before submission";
                    }
                    if (tiers[i].StartsWith("Tier C") && (lowestTier.StartsWith("Tier B") || lowestTier.StartsWith("Tier A") || lowestTier == "")) {
                        lowestTier = "Tier C - Requires review";
                    }
                    if ((tiers[i].StartsWith("Tier B")) && (lowestTier.StartsWith("Tier A") || lowestTier == "")) {
                        lowestTier = "Tier B - May require review";
                    }
                    if ((tiers[i].StartsWith("Tier A")) && (lowestTier == "")) {
                        lowestTier = "Tier A - Excellent";
                    }
                }
            if (lowestTier == "") {
                lowestTier = "Not enough information provided to calculate Tier";
            }
            if (lowestTier.StartsWith("Tier B") && (numRec == "2"))
            {
                lowestTier = "Tier C - Requires review";
            }
            
            return lowestTier; // Return the lowest score found
        } 
    }

}
