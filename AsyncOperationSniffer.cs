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
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.ComponentModel;
using Microsoft.ApplicationInsights.Extensibility;

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
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Use your own server, database, user ID, and password.
            string ConnectionString = System.Environment.GetEnvironmentVariable(
                                    "SQL_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                String sql = "select [count] = count(name), name, statuscode from asyncoperation where statuscode < 30 group by statuscode, name order by [count] desc";
                
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    
                    this.telemetryClient.TrackTrace("Connecting to SQL");
                    conn.Open();
                    
                    command.CommandTimeout = 0;
                    this.telemetryClient.TrackTrace("Executing SQL Command");
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            this.telemetryClient.TrackTrace("Executed SQL Command");
                            while (reader.Read())
                            {
                                MetricTelemetry metricTelemetry = new MetricTelemetry();
                                EventTelemetry eventTelemetry = new EventTelemetry();
                                switch (reader.GetInt32(2))
                                {
                                    case 0: //Waiting for Resources
                                        metricTelemetry.Sum = reader.GetInt32(0);
                                        metricTelemetry.MetricNamespace = "Waiting for Resources";
                                        eventTelemetry.Properties.Add("StatusName", "Waiting for Resources");
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        break;
                                    case 10:
                                        metricTelemetry.Sum = reader.GetInt32(0);
                                        metricTelemetry.MetricNamespace = "Waiting";
                                        eventTelemetry.Properties.Add("StatusName", "Waiting");
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        break;
                                    case 20:
                                        metricTelemetry.Sum = reader.GetInt32(0);
                                        metricTelemetry.MetricNamespace = "In Progress";
                                        eventTelemetry.Properties.Add("StatusName", "In Progress");
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        break;
                                    case 21:
                                        metricTelemetry.Sum = reader.GetInt32(0);
                                        metricTelemetry.MetricNamespace = "Pausing";
                                        eventTelemetry.Properties.Add("StatusName", "Pausing");
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        break;
                                    case 22:
                                        metricTelemetry.Sum = reader.GetInt32(0);
                                        metricTelemetry.MetricNamespace = "Canceling";
                                        eventTelemetry.Properties.Add("StatusName", "Canceling");
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        break;
                                    default:
                                        eventTelemetry.Properties.Add("StatusName", reader.GetInt32(2).ToString());
                                        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                        eventTelemetry.Metrics.Add(reader.GetInt32(2).ToString(), reader.GetInt32(0));
                                        break;
                                }
                                if (!reader.IsDBNull(1))
                                {
                                    metricTelemetry.Name = reader.GetString(1);
                                    eventTelemetry.Name = reader.GetString(1);
                                }
                                //eventTelemetry.Properties.Add("Status", reader.GetInt32(2));
                                this.telemetryClient.TrackMetric(metricTelemetry);
                                this.telemetryClient.TrackEvent(eventTelemetry);
                            }
                            this.telemetryClient.Flush();
                        }
                    }
                    catch(SqlException sqlEx)
                    {
                        this.telemetryClient.TrackException(sqlEx);
                        this.telemetryClient.Flush();
                        throw sqlEx;
                    }
                    catch (Exception ex)
                    {
                        this.telemetryClient.TrackException(ex);
                        this.telemetryClient.Flush();
                        throw ex;
                    }

                    
                }
            }


        }
    }
}
