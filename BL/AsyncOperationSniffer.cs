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
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Dynamics365.OrganizationScanner.DAL;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection.PortableExecutable;

namespace Microsoft.Dynamics365.OrganizationScanner
{
    public class AsyncOperationSniffer
    {

        private readonly TelemetryClient telemetryClient;
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
        public AsyncOperationSniffer()
        {

            string connectionString = System.Environment.GetEnvironmentVariable(
                "APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
            TelemetryConfiguration telemetryConfig = new TelemetryConfiguration();
            telemetryConfig.ConnectionString = connectionString;
            this.telemetryClient = new TelemetryClient(telemetryConfig);
            //this.telemetryClient.InstrumentationKey = key;
            this.telemetryClient.TrackTrace("AsyncOperationSniffer constructor called. Using Environment Variable APPINSIGHTS_INSTRUMENTATIONKEY to get Application Insights Connection String");

        }

        //private async Task<CloudTable> GetTable()
        //{
        //    var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        //    var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        //    var table = tableClient.GetTableReference(TableName);
        //    await table.CreateIfNotExistsAsync();
        //    return table;
        //}

        [FunctionName("AsyncOperationSniffer")]
        public void Run([TimerTrigger("0 */35 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Use your own server, database, user ID, and password.
            string ConnectionString = System.Environment.GetEnvironmentVariable(
                                    "SQL_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            
            SqlDataLayer sqlDataLayer = new SqlDataLayer(log, ConnectionString);

            DTO.AsyncOperationDTO.AsyncOperationResponse asyncOperationResponse = sqlDataLayer.ExecuteAsyncOperationRequest(new DTO.AsyncOperationDTO.AsyncOperationRequest()
            {
                SqlCommand = "select [count] = count(name), name, statuscode from asyncoperation where statuscode < 30 group by statuscode, name order by [count] desc",
                CorrelatonId = Guid.NewGuid().ToString()
            }).Result;
            foreach(DTO.AsyncOperationDTO.AsyncOperation asyncOperation in asyncOperationResponse.AsyncOperations)
            {
                MetricTelemetry metricTelemetry = new MetricTelemetry();
                EventTelemetry eventTelemetry = new EventTelemetry();
                metricTelemetry.Sum = asyncOperation.Count;
                eventTelemetry.Properties.Add("StatusCount", asyncOperation.Count.ToString());
                if (!String.IsNullOrEmpty(asyncOperation.Name))
                {
                    metricTelemetry.Name = asyncOperation.Name;
                    eventTelemetry.Name = asyncOperation.Name;
                }
                switch (asyncOperation.StatusCode)
                {
                    case DTO.AsyncOperationDTO.AsyncOperationStatusCode.WaitingForResources: //Waiting for Resources
                        metricTelemetry.MetricNamespace = "Waiting for Resources";
                        eventTelemetry.Properties.Add("StatusName", "Waiting for Resources");
                        break;
                    case DTO.AsyncOperationDTO.AsyncOperationStatusCode.Waiting:
                        metricTelemetry.MetricNamespace = "Waiting";
                        eventTelemetry.Properties.Add("StatusName", "Waiting");
                        break;
                    case DTO.AsyncOperationDTO.AsyncOperationStatusCode.InProgress:
                        metricTelemetry.MetricNamespace = "In Progress";
                        eventTelemetry.Properties.Add("StatusName", "In Progress");
                        break;
                    case DTO.AsyncOperationDTO.AsyncOperationStatusCode.Pausing:
                        metricTelemetry.MetricNamespace = "Pausing";
                        eventTelemetry.Properties.Add("StatusName", "Pausing");
                        break;
                    case DTO.AsyncOperationDTO.AsyncOperationStatusCode.Canceling:
                        metricTelemetry.MetricNamespace = "Canceling";
                        eventTelemetry.Properties.Add("StatusName", "Canceling");
                        break;
                    default:
                        eventTelemetry.Properties.Add("StatusName", "");
                        eventTelemetry.Properties.Add("StatusCount", asyncOperation.StatusCode.ToString());
                        eventTelemetry.Metrics.Add("", Convert.ToDouble(asyncOperation.StatusCode));
                        break;
                }

                //eventTelemetry.Properties.Add("Status", reader.GetInt32(2));
                this.telemetryClient.TrackMetric(metricTelemetry);
                this.telemetryClient.TrackEvent(eventTelemetry);
            }

        
        this.telemetryClient.Flush();


    }
}
}
