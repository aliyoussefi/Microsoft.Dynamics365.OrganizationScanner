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

namespace Microsoft.Dynamics365.OrganizationScanner
{
    public class SolutionHistoryRecorder
    {
        private readonly TelemetryClient telemetryClient;
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
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
        [FunctionName("SolutionHistoryRecorder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext exCtx,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            string token = ConnectToDynamics(new S2SAuthenticationSettings()
            {
                clientId = clientId,
                clientSecret = clientSecret,
                tenantID = tenantId,
                organizationUrl = orgUrl
            });
            Stopwatch requestTime = new Stopwatch();
            requestTime.Start();
            HttpResponseMessage whoAmIResponse = await ExecuteWhoAmI(log, orgUrl, token);
            requestTime.Stop();
            //rtnObject.Headers.Add("InvocationId", exCtx.InvocationId.ToString());
           
            SendAvailabilityTelemetry(exCtx.InvocationId, exCtx.InvocationId, whoAmIResponse, Convert.ToInt32(requestTime.ElapsedMilliseconds));
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private string ConnectToDynamics(S2SAuthenticationSettings authenticationSettings) {
            ClientCredential clientcred = new ClientCredential(authenticationSettings.clientId, authenticationSettings.clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(authenticationSettings.aadInstance + authenticationSettings.tenantID);
            var authenticationResult = authenticationContext.AcquireTokenAsync(authenticationSettings.organizationUrl, clientcred).Result;
            return authenticationResult.AccessToken;

        }
        public class S2SAuthenticationSettings {
            public string organizationUrl;
            public string clientId;
            public string clientSecret;
            public string aadInstance = "https://login.microsoftonline.com/";
            public string tenantID;
        }

        private async Task<HttpResponseMessage> ExecuteWhoAmI(ILogger log, string dynamicsUrl, string token) {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(dynamicsUrl),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Add this line for TLS complaience
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Call SolutionHistory
            //https://alyousse.crm.dynamics.com/api/data/v9.0/msdyn_solutionhistories?$filter=msdyn_name%20eq%20%27msdynce_LeadManagementAnchor%27

            string queryFilter = "?$filter=";
            string solutionName = string.Empty;
            string startTime = string.Empty;
            if (String.IsNullOrEmpty(solutionName) && String.IsNullOrEmpty(startTime))
            {
                throw new ArgumentNullException("Missing Solution Name and Start Time parameters");
            }
            else if (!String.IsNullOrEmpty(solutionName) && !String.IsNullOrEmpty(startTime)) { queryFilter += "msdyn_name eq '" + solutionName + "' and msdyn_starttime gt '" + startTime + "'"; }
            else
            {
                if (!String.IsNullOrEmpty(solutionName)) { queryFilter += "msdyn_name eq '" + solutionName + "'"; }
                if (!String.IsNullOrEmpty(startTime)) { queryFilter = "msdyn_starttime gt '" + startTime + "'"; }
            }
            var retrieveResponse = await httpClient.GetAsync(String.Format("api/data/v9.0/msdyn_solutionhistories{0}", queryFilter));
            if (retrieveResponse.IsSuccessStatusCode) {
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Content.ReadAsStringAsync().Result);

                var currUserId = (Guid)jRetrieveResponse["UserId"];
                var businessId = (Guid)jRetrieveResponse["BusinessUnitId"];

                
                return retrieveResponse;
            }
            else {
                throw new Exception();
            }
        }

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

    }
}
