using Microsoft.Dynamics365.OrganizationScanner.DTO;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    internal static class DataverseApiDTOMapper
    {
        public static SolutionHistoryDTO.SolutionHistoryRecorderResponse DataverseToDTO(string solutionHistoryResponse)
        {
            SolutionHistoryDTO.SolutionHistoryRecorderResponse rtnObject = new SolutionHistoryDTO.SolutionHistoryRecorderResponse();

            JArray solutions = new JArray();
            JObject responseObject = JObject.Parse(solutionHistoryResponse);
            solutions = (JArray)responseObject["value"];
            if (solutions.Count > 0 ) { 
                rtnObject.SolutionHistories = new List<SolutionHistoryDTO.SolutionHistory>(); 
                foreach (var solution in solutions)
                {
                    rtnObject.SolutionHistories.Add(solution.ToObject<SolutionHistoryDTO.SolutionHistory>());
                }
            }

            return rtnObject;
        }
    }
}
