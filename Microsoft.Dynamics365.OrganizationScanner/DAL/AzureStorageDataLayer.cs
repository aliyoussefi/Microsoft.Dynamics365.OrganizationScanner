using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Dynamics365.OrganizationScanner.DTO.SolutionHistoryDTO;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class AzureStorageDataLayer
    {
        Microsoft.Extensions.Logging.ILogger _logger;
        CloudBlobClient _client;
        public AzureStorageDataLayer(Microsoft.Extensions.Logging.ILogger logger, string azureConnectionString) {
            _logger = logger;
            ConnectToAzureStorage(azureConnectionString);
        }
        private void ConnectToAzureStorage(string azureConnectionString) {
            string storageConnectionString = azureConnectionString;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _client = storageAccount.CreateCloudBlobClient();
        }

        public void WriteToAzureBlobStorage<T>(string containerName, string blobName, T data) where T : List<SolutionHistory> {
            _logger.LogInformation("Connecting to Blob Container " + containerName);
            var container = _client.GetContainerReference(containerName);
            _logger.LogInformation("Getting to Blob Reference " + blobName);
            var blob = container.GetBlockBlobReference(blobName);
            _logger.LogInformation("Opening to Blob Container to write " + blobName);
            using (CloudBlobStream x = blob.OpenWriteAsync().Result)
            {
                _logger.LogInformation("Mapping SolutionHistory count of " + data.Count);
                AzureStorageDataMapper.MapSolutionHistory(data, x, _logger);
                _logger.LogInformation("Flushing SolutionHistory");
                x.Flush();
                x.Close();
            }
        }
    }
}
