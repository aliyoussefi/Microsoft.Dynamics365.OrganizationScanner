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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics365.OrganizationScanner.DAL;
using System.Security;

namespace Microsoft.Dynamics365.OrganizationScanner
{
    public class ApplicationPackageScanner
    {
        private readonly TelemetryClient telemetryClient;
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
        public readonly string username;
        public readonly SecureString password;
        public ApplicationPackageScanner() {
            this.tenantId = System.Environment.GetEnvironmentVariable(
                                    "TENANT_ID", EnvironmentVariableTarget.Process);
            this.orgUrl = System.Environment.GetEnvironmentVariable(
                                    "POWER_PLATFORM_URL", EnvironmentVariableTarget.Process);
            this.clientId = System.Environment.GetEnvironmentVariable(
                                    "POWER_PLATFORM_CLIENT_ID", EnvironmentVariableTarget.Process);
            this.clientSecret = System.Environment.GetEnvironmentVariable(
                                    "POWER_PLATFORM_CLIENT_SECRET", EnvironmentVariableTarget.Process);
            this.username = System.Environment.GetEnvironmentVariable(
                        "POWER_PLATFORM_USERNAME", EnvironmentVariableTarget.Process);
            this.password = new NetworkCredential("",System.Environment.GetEnvironmentVariable(
                        "POWER_PLATFORM_PASSWORD", EnvironmentVariableTarget.Process)).SecurePassword;

            string key = System.Environment.GetEnvironmentVariable(
                "APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
            this.telemetryClient = new TelemetryClient();
            this.telemetryClient.InstrumentationKey = key;
            this.telemetryClient.TrackTrace("MonitorWithApplicationInsightsExample constructor called. Using Environment Variable APPINSIGHTS_INSTRUMENTATIONKEY to get Application Insights Key");

        }
        [FunctionName("GetEnvironmentApplicationPackage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"get", "post", Route = null)] HttpRequest req,
            System.Threading.ExecutionContext exCtx,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string environmentId = req.Query["environmentId"];
            string appInstallState = req.Query["appInstallState"];
            string appName= req.Query["appName"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            PowerPlatformDataLayer powerPlatformDataLayer = new PowerPlatformDataLayer(log);
            string token = powerPlatformDataLayer.ConnectToPowerPlatform(new DAL.UserAuthenticationSettings()
            {
                clientId = clientId,
                username = username,
                password = password,
                tenantID = tenantId,
                organizationUrl = orgUrl
            });
            Stopwatch requestTime = new Stopwatch();
            requestTime.Start();
            DTO.ApplicationPackageDTO.ApplicationStatusResponse applicationStatusResponse = await powerPlatformDataLayer.ExecuteApplicationStatusRequest(new DTO.ApplicationPackageDTO.ApplicationStatusRequest()
            {
                ApplicationInstallState = appInstallState,
                ApplicationName = appName,
                CorrelationId = Guid.NewGuid().ToString(),
                EnvironmentId = environmentId
            });
            requestTime.Stop();
            //DTO.ApplicationPackageDTO.ApplicationPackage rtnObject = applicationStatusResponse.ApplicationPackages.Select(x => x.uniqueName == appName).FirstOrDefault();
            //rtnObject.Headers.Add("InvocationId", exCtx.InvocationId.ToString());
           
            string responseMessage = string.IsNullOrEmpty(environmentId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {environmentId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
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

            // Call WhoAmI
            var retrieveResponse = await httpClient.GetAsync("api/data/v9.0/WhoAmI");
            
            if (retrieveResponse.IsSuccessStatusCode) {
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Content.ReadAsStringAsync().Result);

                var currUserId = (Guid)jRetrieveResponse["UserId"];
                var businessId = (Guid)jRetrieveResponse["BusinessUnitId"];

                log.LogInformation("My User Id – " + currUserId);
                log.LogInformation("My Business Id – " + businessId);
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
