/*
# This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. 
# THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
# INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
# We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that. 
# You agree: 
# (i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded; 
# (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; 
# and (iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code 
*/
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics365.OrganizationScanner.DAL;
using System.Runtime.Serialization;
using Microsoft.OData.Edm;
using static Microsoft.Dynamics365.OrganizationScanner.DAL.DataverseDataLayer;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Organization;
using System.Drawing;

namespace Microsoft.Dynamics365.OrganizationScanner
{
    public class SolutionHistoryRecorder
    {
        #region DTO
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
        #endregion DTO
        #region class props
        private readonly TelemetryClient telemetryClient;
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
        #endregion
        #region ctor
        public SolutionHistoryRecorder() {
            this.tenantId = System.Environment.GetEnvironmentVariable(
                                    "TENANT_ID", EnvironmentVariableTarget.Process);
            this.orgUrl = System.Environment.GetEnvironmentVariable(
                                    "ORG_URL", EnvironmentVariableTarget.Process);
            this.clientId = System.Environment.GetEnvironmentVariable(
                                    "CLIENT_ID", EnvironmentVariableTarget.Process);
            this.clientSecret = System.Environment.GetEnvironmentVariable(
                                    "CLIENT_SECRET", EnvironmentVariableTarget.Process);

            string key = System.Environment.GetEnvironmentVariable(
                "APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
            ApplicationInsightsDataLayer logger = new ApplicationInsightsDataLayer(key);
            logger.Log("MonitorWithApplicationInsightsExample constructor called. Using Environment Variable APPINSIGHTS_INSTRUMENTATIONKEY to get Application Insights Key");

        }
        #endregion
        #region Functions
        [FunctionName("SolutionHistoryRecorder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext exCtx,
            ILogger log)
        {
            log.LogInformation("Running SolutionHistoryRecorder.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<SolutionHistoryRecorderRequest>(requestBody);
            //name = name ?? data?.name;

            DataverseDataLayer dataverse = new DataverseDataLayer(log, true);

            string token = dataverse.ConnectToDynamics(new S2SAuthenticationSettings()
            {
                clientId = clientId,
                clientSecret = clientSecret,
                tenantID = tenantId,
                organizationUrl = orgUrl
            });
            SolutionHistoryRecorderRequest solutionHistoryRecorderRequest = new SolutionHistoryRecorderRequest()
            {
                SolutionName = data.SolutionName,
                StartTime = data.StartTime
            };
            Stopwatch requestTime = new Stopwatch();
            requestTime.Start();
            SolutionHistoryRecorderResponse ExecuteSolutionHistoryResponse = await dataverse.ExecuteSolutionHistoryRequest(solutionHistoryRecorderRequest);
            requestTime.Stop();
            log.LogInformation("ExecuteSolutionHistoryRequest took " + requestTime.ElapsedMilliseconds + " ms.");
            
            log.LogInformation("Found " + ExecuteSolutionHistoryResponse.SolutionHistories.Count + " for solution.");
            //rtnObject.Headers.Add("InvocationId", exCtx.InvocationId.ToString());
            foreach (SolutionHistory solutionHistory in ExecuteSolutionHistoryResponse.SolutionHistories)
            {
                //Log or store in table...
                //log.LogInformation(String.Format("{0} Solution History", data.SolutionName), solutionHistory.msdyn_endtime));
            }
            string responseMessage = string.IsNullOrEmpty(data.SolutionName)
                ? "Soluton History Recorded completed successfully. Found " + ExecuteSolutionHistoryResponse.SolutionHistories.Count() + " for solution " + data.SolutionName
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
        #endregion
        #region Helper methods
        public HttpResponseMessage SendAvailabilityTelemetry(Guid operationId, Guid requestId, HttpResponseMessage response, int duration) {
            AvailabilityTelemetry availability = new AvailabilityTelemetry();
            DateTime dt = DateTime.Now;
            availability.Duration = new TimeSpan(0, 0, 0, 0, duration);
            availability.Id = requestId.ToString();
            availability.Message = (response.StatusCode <= System.Net.HttpStatusCode.PartialContent) ?
                    "Passed" :
                    response.StatusCode.ToString() + " " + response.ReasonPhrase;
            availability.Name = "Availability Test of Dynamics 365";
            availability.Success = response.IsSuccessStatusCode;
            foreach (var header in response.Headers) {
                availability.Properties.Add(header.Key, header.Value.First().ToString());
            }
            availability.Timestamp = dt;
            telemetryClient.Context.Operation.Id = operationId.ToString();
            telemetryClient.TrackAvailability(availability);
            telemetryClient.Flush();
            return response;
        }
        #endregion
    }
}
