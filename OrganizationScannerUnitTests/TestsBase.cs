using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationScannerUnitTests
{
    public class TestsBase
    {
        public TestsBase() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfiguration config = builder.Build();

            Environment.SetEnvironmentVariable("TENANT_ID", config["Values:TENANT_ID"]);
            Environment.SetEnvironmentVariable("ORG_URL", config["Values:ORG_URL"]);
            Environment.SetEnvironmentVariable("CLIENT_ID", config["Values:CLIENT_ID"]);
            Environment.SetEnvironmentVariable("CLIENT_SECRET", config["Values:CLIENT_SECRET"]);
            Environment.SetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING", config["Values:AZURE_STORAGE_CONNECTION_STRING"]);
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", config["Values:APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }
    }
}
