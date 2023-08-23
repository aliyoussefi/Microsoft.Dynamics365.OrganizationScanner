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
    internal static class PowerPlatformAdminDataMapper
    {
        public static ApplicationPackageDTO.ApplicationStatusResponse GetEnvironmentApplicationPackageToDTO(string solutionHistoryResponse)
        {
            ApplicationPackageDTO.ApplicationStatusResponse rtnObject = new ApplicationPackageDTO.ApplicationStatusResponse();

            JArray solutions = new JArray();
            JObject responseObject = JObject.Parse(solutionHistoryResponse);
            solutions = (JArray)responseObject["value"];
            if (solutions.Count > 0 ) { 
                rtnObject.ApplicationPackages = new List<ApplicationPackageDTO.ApplicationPackage>(); 
                foreach (var solution in solutions)
                {
                    rtnObject.ApplicationPackages.Add(solution.ToObject<ApplicationPackageDTO.ApplicationPackage>());
                }
            }

            return rtnObject;
        }
    }
}
