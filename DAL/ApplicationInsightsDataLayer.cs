using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class ApplicationInsightsDataLayer
    {
        private TelemetryClient telemetryClient;
        public enum Severity {Informational, Warning, Error, Critical }
        public ApplicationInsightsDataLayer(string appInsightsConnectionString) {
            TelemetryConfiguration telemetryConfig = new TelemetryConfiguration();
            telemetryConfig.ConnectionString = appInsightsConnectionString;
            this.telemetryClient = new TelemetryClient(telemetryConfig);
        }

        public void Log(string message)
        {
            this.telemetryClient.TrackTrace(message, SeverityLevel.Information);
        }

        public void Log(string message, Severity severityLevel)
        {

            this.telemetryClient.TrackTrace(message, (SeverityLevel)severityLevel);
        }

        public void LogError(string message)
        {
            this.telemetryClient.TrackException(new Exception(message));
        }

        public void LogError(Exception exception)
        {
            this.telemetryClient.TrackException(exception);
        }
    }
}
