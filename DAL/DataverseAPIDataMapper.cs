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
        public static SolutionHistoryRecorder.SolutionHistoryRecorderResponse DataverseToDTO(string solutionHistoryResponse)
        {
            SolutionHistoryRecorder.SolutionHistoryRecorderResponse rtnObject = new SolutionHistoryRecorder.SolutionHistoryRecorderResponse();

            JArray solutions = new JArray();
            JObject responseObject = JObject.Parse(solutionHistoryResponse);
            solutions = (JArray)responseObject["value"];
            if (solutions.Count > 0 ) { 
                rtnObject.SolutionHistories = new List<SolutionHistoryRecorder.SolutionHistory>(); 
                foreach (var solution in solutions)
                {
                    rtnObject.SolutionHistories.Add(solution.ToObject<SolutionHistoryRecorder.SolutionHistory>());
                }
            }

            return rtnObject;
        }
    }
}
