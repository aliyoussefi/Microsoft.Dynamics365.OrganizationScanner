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
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Dynamics365.OrganizationScanner.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.Dynamics365.OrganizationScanner.DTO.SolutionHistoryDTO;

namespace Microsoft.Dynamics365.OrganizationScanner
{
    public class SolutionHistoryRecorder
    {
        #region class props
        private readonly TelemetryClient telemetryClient;
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
        public readonly string _azureStorageConnectionString;
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
            this._azureStorageConnectionString = System.Environment.GetEnvironmentVariable(
                                    "AZURE_STORAGE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
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
            System.Threading.ExecutionContext exCtx,
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

            AzureStorageDataLayer azureStorageData = new AzureStorageDataLayer(log, this._azureStorageConnectionString);
            azureStorageData.WriteToAzureBlobStorage(@"dataverse-90alitestins-alyoussefnaos\solutionhistory", "data" +DateTime.UtcNow.ToString("MM-dd-yyyy-H-mm-ss") + ".csv", ExecuteSolutionHistoryResponse.SolutionHistories);

            string responseMessage = string.IsNullOrEmpty(data.SolutionName)
            ? "Soluton History Recorded completed successfully. Found " + ExecuteSolutionHistoryResponse.SolutionHistories.Count() + " for solution " + data.SolutionName
            : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("SolutionHistoryWriteTodaysHistory")]
        public void WriteTodaysHistory(
    [TimerTrigger("55 23 * * * ")] TimerInfo myTimer,
    System.Threading.ExecutionContext exCtx,
    ILogger log)
        {
            log.LogInformation("Running SolutionHistoryWriteTodaysHistory.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject<SolutionHistoryRecorderRequest>(requestBody);
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
                //SolutionName = data.SolutionName,
                StartTime = DateTime.Today.ToString()
            };
            Stopwatch requestTime = new Stopwatch();
            requestTime.Start();
            SolutionHistoryRecorderResponse ExecuteSolutionHistoryResponse = dataverse.ExecuteSolutionHistoryRequest(solutionHistoryRecorderRequest).Result;
            requestTime.Stop();
            log.LogInformation("ExecuteSolutionHistoryRequest took " + requestTime.ElapsedMilliseconds + " ms.");

            log.LogInformation("Found " + ExecuteSolutionHistoryResponse.SolutionHistories.Count + " for solution.");
            //rtnObject.Headers.Add("InvocationId", exCtx.InvocationId.ToString());
            //foreach (SolutionHistory solutionHistory in ExecuteSolutionHistoryResponse.SolutionHistories)
            //{
            //    //Log or store in table...
            //    //log.LogInformation(String.Format("{0} Solution History", data.SolutionName), solutionHistory.msdyn_endtime));
            //}
            AzureStorageDataLayer azureStorageData = new AzureStorageDataLayer(log, this._azureStorageConnectionString);
            azureStorageData.WriteToAzureBlobStorage(@"dataverse-90alitestins-alyoussefnaos\solutionhistory", "data" + DateTime.UtcNow.ToString("MM-dd-yyyy-H-mm-ss") + ".csv", ExecuteSolutionHistoryResponse.SolutionHistories);

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
