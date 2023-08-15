using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DTO
{
    public class SolutionHistoryDTO
    {
        [DataContract]
        public class SolutionHistoryRecorderRequest
        {
            [DataMember]
            public string SolutionName { get; set; }
            [DataMember]
            public string StartTime { get; set; }

        }
        public class SolutionHistoryRecorderResponse
        {
            [DataMember]
            public string SolutionName { get; set; }
            [DataMember]
            public string StartTime { get; set; }
            [DataMember]
            public List<SolutionHistory> SolutionHistories { get; set; }

        }

        public class SolutionHistory
        {
            [DataMember]
            public string msdyn_solutionhistoryid { get; set; }
            [DataMember]
            public string msdyn_name { get; set; }
            [DataMember]
            public string msdyn_correlationid { get; set; }
            [DataMember]
            public string msdyn_endtime { get; set; }
            [DataMember]
            public string msdyn_errorcode { get; set; }
            [DataMember]
            public string msdyn_exceptionmessage { get; set; }
            [DataMember]
            public string msdyn_exceptionstack { get; set; }
            [DataMember]
            public string msdyn_ismanaged { get; set; }
            [DataMember]
            public string msdyn_ispatch { get; set; }
            [DataMember]
            public string msdyn_operation { get; set; }
            [DataMember]
            public string msdyn_packagename { get; set; }
            [DataMember]
            public string msdyn_publishername { get; set; }
            [DataMember]
            public string msdyn_solutionid { get; set; }
            [DataMember]
            public string msdyn_solutionversion { get; set; }
            [DataMember]
            public string msdyn_starttime { get; set; }
            [DataMember]
            public string msdyn_status { get; set; }
            [DataMember]
            public string msdyn_suboperation { get; set; }
            [DataMember]
            public string msdyn_result { get; set; }
            [DataMember]
            public string msdyn_totaltime { get; set; }
            [DataMember]
            public string msdyn_publisherid { get; set; }
            [DataMember]
            public string msdyn_retrycount { get; set; }
            [DataMember]
            public string msdyn_packageversion { get; set; }
            [DataMember]
            public string msdyn_maxretries { get; set; }

        }
    }
}
