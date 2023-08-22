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
using System.Drawing;
using static Microsoft.Dynamics365.OrganizationScanner.DTO.SolutionHistoryDTO;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

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

            string storageConnectionString = this._azureStorageConnectionString;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(@"dataverse-90alitestins-alyoussefnaos\solutionhistory");
            var blob = container.GetBlockBlobReference("data" + DateTime.UtcNow.ToString("MM-dd-yyyy-H-mm-ss") + ".csv");
            using (CloudBlobStream x = blob.OpenWriteAsync().Result)
            {
                foreach (var rec in ExecuteSolutionHistoryResponse.SolutionHistories)
                {
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_endtime + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_errorcode + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionmessage + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionstack + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ismanaged + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ispatch + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_maxretries + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_name + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_operation + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packagename + ","));


                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packageversion + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publisherid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publishername + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_result + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_retrycount + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionhistoryid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionversion + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_starttime + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_status + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_suboperation + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_totaltime + ""));
                    x.Write(System.Text.Encoding.Default.GetBytes("\n"));
                }
                x.Flush();
               x.Close();
            }
            string responseMessage = string.IsNullOrEmpty(data.SolutionName)
                ? "Soluton History Recorded completed successfully. Found " + ExecuteSolutionHistoryResponse.SolutionHistories.Count() + " for solution " + data.SolutionName
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("SolutionHistoryWriteTodaysHistory")]
        public void WriteTodaysHistory(
    [TimerTrigger("55 23 * * * ")] TimerInfo myTimer,
    ExecutionContext exCtx,
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
            string storageConnectionString = this._azureStorageConnectionString;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(@"dataverse-90alitestins-alyoussefnaos\solutionhistory");
            var blob = container.GetBlockBlobReference("data" + DateTime.UtcNow.ToString("MM-dd-yyyy-H-mm-ss") + ".csv");
            using (CloudBlobStream x = blob.OpenWriteAsync().Result)
            {
                foreach (var rec in ExecuteSolutionHistoryResponse.SolutionHistories)
                {
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_endtime + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_errorcode + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionmessage + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionstack + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ismanaged + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ispatch + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_maxretries + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_name + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_operation + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packagename + ","));


                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packageversion + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publisherid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publishername + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_result + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_retrycount + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionhistoryid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionid + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionversion + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_starttime + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_status + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_suboperation + ","));
                    x.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_totaltime + ""));
                    x.Write(System.Text.Encoding.Default.GetBytes("\n"));
                }
                x.Flush();
                x.Close();
            }

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
