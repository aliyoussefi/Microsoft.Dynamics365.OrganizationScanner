using Microsoft.Dynamics365.OrganizationScanner.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DTO
{
    public class AsyncOperationDTO
    {


        [DataContract]
        public class AsyncOperationRequest : ISqlDTO
        {
            [DataMember]
            public string SqlCommand { get; set; }
            [DataMember]
            public string SolutionName { get; set; }
            [DataMember]
            public string CorrelatonId { get; set; }
        }

        [DataContract]
        public class AsyncOperationResponse
        {
            [DataMember]
            public string CorrelatonId { get; set; }
            [DataMember]
            public List<AsyncOperation> AsyncOperations { get; set; }
        }
        [DataContract]
        public class AsyncOperation
        {
            [DataMember]
            public Int32 Count { get; set; }
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public AsyncOperationStatusCode StatusCode { get; set; }
            [DataMember]
            public string StatusName { get; set; }
        }
        [Flags]
        public enum AsyncOperationStatusCode {
            [Display(Name = "Waiting for Resources")]
            WaitingForResources=0,
            [Display(Name = "Waiting")]
            Waiting = 10,
            [Display(Name = "In Progress")]
            InProgress = 20,
            [Display(Name = "Pausing")]
            Pausing = 21,
            [Display(Name = "Canceling")]
            Canceling = 22

        }
    }
}
