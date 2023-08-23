using Microsoft.Dynamics365.OrganizationScanner.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class PowerPlatformDataLayer
    {
        private string _orgUrl = "";
        private string _clientId = "";
        private SecureString _clientSecret = null;
        Microsoft.Extensions.Logging.ILogger _logger = null;
        private string _token = null;
        public PowerPlatformDataLayer(Microsoft.Extensions.Logging.ILogger logger) {
            _logger = logger;
        }


        public string ConnectToPowerPlatform(S2SAuthenticationSettings authenticationSettings)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(authenticationSettings.organizationUrl),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

            var values = new Dictionary<string, string>
                  {
                      { "client_id", authenticationSettings.clientId },
                      { "scope", "https://api.powerplatform.com/.default" },
                      { "client_secret", authenticationSettings.clientSecret },
                    {"grant_type", "client_credentials" }
                  };

            var content = new FormUrlEncodedContent(values);
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Add this line for TLS compliance
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var response = httpClient.PostAsync("https://login.microsoftonline.com/" + authenticationSettings.tenantID + "/oauth2/v2.0/token", content);

            var responseString = response.Result.Content.ReadAsStringAsync();
            S2SAuthenticationResponse responseObject = JObject.Parse(responseString.Result).ToObject<S2SAuthenticationResponse>();
            _orgUrl = authenticationSettings.organizationUrl;
            _token = responseObject.access_token;
            return responseObject.access_token;

        }
        public string ConnectToPowerPlatform(UserAuthenticationSettings authenticationSettings)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(authenticationSettings.organizationUrl),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

            var values = new Dictionary<string, string>
                  {
                      { "client_id", authenticationSettings.clientId },
                      { "scope", "https://api.powerplatform.com/.default" },
                      { "username", authenticationSettings.username },
                    { "password", SecureStringToString(authenticationSettings.password) },
                    {"grant_type", "password" }
                  };

            var content = new FormUrlEncodedContent(values);
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Add this line for TLS compliance
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var response = httpClient.PostAsync("https://login.microsoftonline.com/" + authenticationSettings.tenantID + "/oauth2/v2.0/token", content);

            var responseString = response.Result.Content.ReadAsStringAsync();
            S2SAuthenticationResponse responseObject = JObject.Parse(responseString.Result).ToObject<S2SAuthenticationResponse>();
            _orgUrl = authenticationSettings.organizationUrl;
            _token = responseObject.access_token;
            return responseObject.access_token;

        }
        private String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
        #region DTO Methods
        public async Task<ApplicationPackageDTO.ApplicationStatusResponse> ExecuteApplicationStatusRequest(ApplicationPackageDTO.ApplicationStatusRequest request)
        {
            SolutionHistoryDTO.SolutionHistoryRecorderResponse response = null;
            return ExecuteGetEnvironmentApplicationAPI(request);
        }
        private ApplicationPackageDTO.ApplicationStatusResponse ExecuteGetEnvironmentApplicationAPI(ApplicationPackageDTO.ApplicationStatusRequest request)
        {
            ApplicationPackageDTO.ApplicationStatusResponse response = null;
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.powerplatform.com/"),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Add this line for TLS compliance
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string queryFilter = "";
            string environmentId = request.EnvironmentId;
            string appInstallState = request.ApplicationInstallState;
            if (String.IsNullOrEmpty(environmentId) && String.IsNullOrEmpty(appInstallState))
            {
                throw new ArgumentNullException("Missing EnvironmentId and appInstallState parameters");
            }
            else if (!String.IsNullOrEmpty(environmentId) && !String.IsNullOrEmpty(appInstallState)) {
                queryFilter += String.Format("appmanagement/environments/{0}/applicationPackages?appInstallState={1}&api-version=2022-03-01-preview", request.EnvironmentId, request.ApplicationInstallState);
            }
            else
            {
                queryFilter += String.Format("appmanagement/environments/{0}/applicationPackages?api-version=2022-03-01-preview", request.EnvironmentId);
            }
  
            //https://learn.microsoft.com/en-us/rest/api/power-platform/appmanagement/applications/get-environment-application-package
            var retrieveResponse = httpClient.GetAsync(queryFilter);
            if (retrieveResponse.Result.IsSuccessStatusCode)
            {
                var responseContent = retrieveResponse.Result.Content.ReadAsStringAsync();
                response = PowerPlatformAdminDataMapper.GetEnvironmentApplicationPackageToDTO(responseContent.Result);
                return response;
            }
            else
            {
                _logger.LogError($"Query operation failed for {_orgUrl}:\nReason: {retrieveResponse.Result.ReasonPhrase}");
                throw new WebException($"Query operation failed for {_orgUrl}:\nReason: {retrieveResponse.Result.ReasonPhrase}");
            }
        }
        #endregion

    }
}
