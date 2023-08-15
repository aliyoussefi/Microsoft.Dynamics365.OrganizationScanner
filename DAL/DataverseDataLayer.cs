using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Security;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json.Linq;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Dynamics365.OrganizationScanner.DTO;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class DataverseDataLayer
    {
        private bool _preferSdk = true;
        private string _orgUrl = "";
        private string _clientId = "";
        private SecureString _clientSecret = null;
        Xrm.Sdk.PluginTelemetry.ILogger _dataverseLogger = null;
        Microsoft.Extensions.Logging.ILogger _logger = null;
        ServiceClient _serviceClient = null;
        private string _token = null;
        public DataverseDataLayer(Microsoft.Extensions.Logging.ILogger logger, bool preferSdk) {
            _preferSdk = preferSdk;
            _logger = logger;
        }


        public string ConnectToDynamics(S2SAuthenticationSettings authenticationSettings)
        {
            ClientCredential clientcred = new ClientCredential(authenticationSettings.clientId, authenticationSettings.clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(authenticationSettings.aadInstance + authenticationSettings.tenantID);
            var authenticationResult = authenticationContext.AcquireTokenAsync(authenticationSettings.organizationUrl, clientcred).Result;
            _token = authenticationResult.AccessToken;

            Microsoft.PowerPlatform.Dataverse.Client.ServiceClient dataverseClient = new ServiceClient(new Uri(authenticationSettings.organizationUrl), authenticationSettings.clientId, authenticationSettings.clientSecret, true, _logger);
            _serviceClient = dataverseClient;
            return authenticationResult.AccessToken;

        }



        #region DTO Methods
        public async Task<SolutionHistoryDTO.SolutionHistoryRecorderResponse> ExecuteSolutionHistoryRequest(SolutionHistoryDTO.SolutionHistoryRecorderRequest request)
        {
            SolutionHistoryDTO.SolutionHistoryRecorderResponse response = null;
            if (_preferSdk)
            {
                return ExecuteSolutionHistoryRequestSDK(request);
            }
            else
            {
                return ExecuteSolutionHistoryRequestAPI(request);
            }
        }
        private SolutionHistoryDTO.SolutionHistoryRecorderResponse ExecuteSolutionHistoryRequestAPI(SolutionHistoryDTO.SolutionHistoryRecorderRequest request)
        {
            SolutionHistoryDTO.SolutionHistoryRecorderResponse response = null;
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_orgUrl),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Add this line for TLS compliance
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Call SolutionHistory
            //https://alyousse.crm.dynamics.com/api/data/v9.0/msdyn_solutionhistories?$filter=msdyn_name%20eq%20%27msdynce_LeadManagementAnchor%27

            string queryFilter = "?$filter=";
            string solutionName = request.SolutionName;
            string startTime = request.StartTime;
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
            var retrieveResponse = httpClient.GetAsync(String.Format("api/data/v9.0/msdyn_solutionhistories{0}", queryFilter));
            if (retrieveResponse.Result.IsSuccessStatusCode)
            {

                var responseContent = retrieveResponse.Result.Content.ReadAsStringAsync();
                response = DataverseApiDTOMapper.DataverseToDTO(responseContent.Result);
                return response;
            }
            else
            {
                _logger.LogError($"Query operation failed for {_orgUrl}:\nReason: {retrieveResponse.Result.ReasonPhrase}");
                throw new WebException($"Query operation failed for {_orgUrl}:\nReason: {retrieveResponse.Result.ReasonPhrase}");
            }
        }
        private SolutionHistoryDTO.SolutionHistoryRecorderResponse ExecuteSolutionHistoryRequestSDK(SolutionHistoryDTO.SolutionHistoryRecorderRequest request)
        {
            SolutionHistoryDTO.SolutionHistoryRecorderResponse response = null;
            
            string queryFilter = "?$filter=";
            string solutionName = request.SolutionName;
            string startTime = request.StartTime;
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

            try
            {

                var httpResponse = _serviceClient.ExecuteWebRequest(HttpMethod.Get, String.Format("msdyn_solutionhistories{0}", queryFilter), "", null);

                if (httpResponse.IsSuccessStatusCode)
                {

                    var responseContent = httpResponse.Content.ReadAsStringAsync();
                    response = DataverseApiDTOMapper.DataverseToDTO(responseContent.Result);
                    return response;
                }
                else
                {
                   _logger.LogError($"Query operation failed for {_orgUrl}:\nReason: {httpResponse.ReasonPhrase}");
                    throw new WebException($"Query operation failed for {_orgUrl}:\nReason: {httpResponse.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Query operation failed for {_orgUrl}:\nMessage: {ex.Message}\nDetail: {ex.InnerException}");
                throw ex;
            }
        }
        #endregion

    }
}
