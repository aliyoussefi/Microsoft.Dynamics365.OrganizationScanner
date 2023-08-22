using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Data.SqlClient;
using Microsoft.Dynamics365.OrganizationScanner.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using static Microsoft.Dynamics365.OrganizationScanner.DTO.AsyncOperationDTO;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class SqlDataLayer
    {
        private string _connString = "";
        Microsoft.Extensions.Logging.ILogger _logger = null;
        private SqlConnection _conn = null;
        public SqlDataLayer(Microsoft.Extensions.Logging.ILogger logger, string connString) {
            _logger = logger;
            _connString = connString;
            _conn = ConnectToSqlDatabase(_connString);
        }

        private SqlConnection ConnectToSqlDatabase(string connString)
        {
            return new SqlConnection(connString);
        }

        #region DTO Methods
        public async Task<AsyncOperationDTO.AsyncOperationResponse> ExecuteAsyncOperationRequest(AsyncOperationDTO.AsyncOperationRequest request)
        {
            AsyncOperationDTO.AsyncOperationResponse response = null;
            using (_conn)
            {

                using (SqlCommand command = new SqlCommand(request.SqlCommand, _conn))
                {

                    this._logger.LogInformation("Connecting to SQL");
                    _conn.Open();

                    command.CommandTimeout = 0;
                    this._logger.LogInformation("Executing SQL Command");
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            this._logger.LogInformation("Executed SQL Command. Returned " + reader.FieldCount + " columns.");
                            response.AsyncOperations = new List<AsyncOperationDTO.AsyncOperation>();
                            while (reader.Read())
                            {
                                response.AsyncOperations.Add(new AsyncOperationDTO.AsyncOperation()
                                {
                                    StatusCode = reader.IsDBNull(reader.GetInt32(2)) ? AsyncOperationStatusCode.Unknown : (AsyncOperationStatusCode)reader.GetInt32(2),
                                    //StatusName = (AsyncOperationStatusCode)reader.GetInt32(2).ToString(),
                                    Name = reader.GetString(1),
                                    Count = reader.IsDBNull(reader.GetInt32(0)) ? 0 : reader.GetInt32(0)
                                });
                                //MetricTelemetry metricTelemetry = new MetricTelemetry();
                                //EventTelemetry eventTelemetry = new EventTelemetry();
                                //switch (reader.GetInt32(2))
                                //{
                                //    case 0: //Waiting for Resources
                                //        metricTelemetry.Sum = reader.GetInt32(0);
                                //        metricTelemetry.MetricNamespace = "Waiting for Resources";
                                //        eventTelemetry.Properties.Add("StatusName", "Waiting for Resources");
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        break;
                                //    case 10:
                                //        metricTelemetry.Sum = reader.GetInt32(0);
                                //        metricTelemetry.MetricNamespace = "Waiting";
                                //        eventTelemetry.Properties.Add("StatusName", "Waiting");
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        break;
                                //    case 20:
                                //        metricTelemetry.Sum = reader.GetInt32(0);
                                //        metricTelemetry.MetricNamespace = "In Progress";
                                //        eventTelemetry.Properties.Add("StatusName", "In Progress");
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        break;
                                //    case 21:
                                //        metricTelemetry.Sum = reader.GetInt32(0);
                                //        metricTelemetry.MetricNamespace = "Pausing";
                                //        eventTelemetry.Properties.Add("StatusName", "Pausing");
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        break;
                                //    case 22:
                                //        metricTelemetry.Sum = reader.GetInt32(0);
                                //        metricTelemetry.MetricNamespace = "Canceling";
                                //        eventTelemetry.Properties.Add("StatusName", "Canceling");
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        break;
                                //    default:
                                //        eventTelemetry.Properties.Add("StatusName", reader.GetInt32(2).ToString());
                                //        eventTelemetry.Properties.Add("StatusCount", reader.GetInt32(0).ToString());
                                //        eventTelemetry.Metrics.Add(reader.GetInt32(2).ToString(), reader.GetInt32(0));
                                //        break;
                                //}
                                //if (!reader.IsDBNull(1))
                                //{
                                //    metricTelemetry.Name = reader.GetString(1);
                                //    eventTelemetry.Name = reader.GetString(1);
                                //}
                                ////eventTelemetry.Properties.Add("Status", reader.GetInt32(2));
                                //this.telemetryClient.TrackMetric(metricTelemetry);
                                //this.telemetryClient.TrackEvent(eventTelemetry);
                            }
                            //this.telemetryClient.Flush();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        this._logger.LogError(sqlEx, sqlEx.Message);
                        throw sqlEx;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, ex.Message);
                        throw ex;
                    }
                    return response;

                }
            }
        }
        #endregion

    }
}
