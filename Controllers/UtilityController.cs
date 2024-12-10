using ClinicalResearchApp.Data;
using ClinicalResearchApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ClinicalResearchApp.Controllers
{
    public class UtilityController : Controller
        {
        public class RequestModel
        {

             public required string[][] tierArray { get; set; }  // This holds the 2D array
            public required string dataLeavingJHM { get; set; }                // Additional parameter
            public required string numRecords { get; set; }           // Additional parameter
            public required string consentYN { get; set; }             // Additional parameter
            public required string humanDataSharingLevel { get; set; } // Additional parameter
            public required string involvesSensitiveHealthInfo { get; set; } // Additional parameter
            public required string dataSharingAgreement { get; set; } // Additional parameter

        }

            [HttpPost]
            public String GetTierLevel([FromBody] RequestModel requestData) {

                // Process the array data
                if (requestData == null)
                {
                    return "No data provided";
                } else {
                    Console.WriteLine("The array is not null");
            
                    string[][] jaggedArray = requestData.tierArray;
                    string[,] array = ConvertJaggedTo2D(jaggedArray);
                    string dataLeavingJHM = requestData.dataLeavingJHM;
                    string numRecords = requestData.numRecords;
                    string consentYN = requestData.consentYN;  
                    string humanDataSharingLevel = requestData.humanDataSharingLevel;
                    string involvesSensitiveHealthInfo = requestData.involvesSensitiveHealthInfo;
                    string dataSharingAgreement = requestData.dataSharingAgreement;


                    string tier = Utility.CalculateTier(array, dataLeavingJHM, numRecords, consentYN, humanDataSharingLevel, involvesSensitiveHealthInfo, dataSharingAgreement);
                
                    if (tier == null)
                    {
                        return "UtilityController - Not enough info to calculate Tier";
                    } else {
                        return tier;
                    }
                }

            }

            

            public string[,] ConvertJaggedTo2D(string[][] jaggedArray)
{
                // Get the dimensions of the jagged array
                int rows = jaggedArray.Length;
                int cols = jaggedArray[0].Length; // Assuming all inner arrays are the same length

                // Create a new 2D array with the same dimensions
                string[,] result = new string[rows, cols];

                // Loop through the jagged array and copy the elements to the 2D array
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        result[i, j] = jaggedArray[i][j];
                    }
                }

                return result;
}
            
        }
}
