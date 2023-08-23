using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DTO
{
    public class ApplicationPackageDTO
    {
        [DataContract]
        public class ApplicationStatusRequest
        {
            [DataMember]
            public string CorrelationId { get; set; }
            [DataMember]
            public string ApplicationName { get; set; }
            [DataMember]
            public string ApplicationInstallState { get; set; }
            [DataMember]
            public string EnvironmentId { get; set; }

        }
        [DataContract]
        public class ApplicationStatusResponse
        {
            [DataMember]
            public string CorrelationId { get; set; }
            [DataMember]
            public string NextLink { get; set; }
            [DataMember]
            public List<ApplicationPackage> ApplicationPackages { get; set; }

        }
        //https://learn.microsoft.com/en-us/rest/api/power-platform/appmanagement/applications/get-environment-application-package#applicationpackage
        [DataContract]
        public class ApplicationPackage
        {
            [DataMember]
            public string applicationDescription { get; set; }
            [DataMember]
            public string applicationId { get; set; }
            [DataMember]
            public string applicationName { get; set; }
            [DataMember]
            public string applicationVisibility { get; set; }

            [DataMember]
            public string catalogVisibility { get; set; }
            [DataMember]
            public string customHandleUpgrade { get; set; }
            [DataMember]
            public string endDateUtc { get; set; }
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string instancePackageId { get; set; }
            [DataMember]
            public string lastError { get; set; }
            [DataMember]
            public string learnMoreUrl { get; set; }

            [DataMember]
            public string localizedDescription { get; set; }
            [DataMember]
            public string localizedName { get; set; }
            [DataMember]
            public string platformMaxVersion { get; set; }
            [DataMember]
            public string platformMinVersion { get; set; }
            [DataMember]
            public string publisherId { get; set; }
            [DataMember]
            public string publisherName { get; set; }
            [DataMember]
            public string singlePageApplicationUrl { get; set; }
            [DataMember]
            public string startDateUtc { get; set; }
            [DataMember]
            public string state { get; set; }
            //[DataMember]
            //public string supportedCountries { get; set; }
            [DataMember]
            public string uniqueName { get; set; }
            [DataMember]
            public string version { get; set; }
        }
    }
}
