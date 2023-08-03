using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class DataverseDataLayer
    {
        private string orgUrl = "";
        private string clientId = "";
        private SecureString clientSecret = null;
        public DataverseDataLayer(string orgUrl, string clientId, SecureString clientSecret) { }


        private string ConnectToDynamics(S2SAuthenticationSettings authenticationSettings)
        {
            ClientCredential clientcred = new ClientCredential(authenticationSettings.clientId, authenticationSettings.clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(authenticationSettings.aadInstance + authenticationSettings.tenantID);
            var authenticationResult = authenticationContext.AcquireTokenAsync(authenticationSettings.organizationUrl, clientcred).Result;
            return authenticationResult.AccessToken;

        }
        public class S2SAuthenticationSettings
        {
            public string organizationUrl;
            public string clientId;
            public string clientSecret;
            public string aadInstance = "https://login.microsoftonline.com/";
            public string tenantID;
        }
    }
}
