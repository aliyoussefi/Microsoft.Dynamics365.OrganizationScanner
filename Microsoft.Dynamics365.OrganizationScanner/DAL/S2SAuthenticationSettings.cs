using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public class UserAuthenticationSettings
    {
        public string organizationUrl;
        public string clientId;
        public string username;
        public SecureString password;
        public string aadInstance = "https://login.microsoftonline.com/";
        public string tenantID;
    }
    public class S2SAuthenticationSettings
    {
        public string organizationUrl;
        public string clientId;
        public string clientSecret;
        public string aadInstance = "https://login.microsoftonline.com/";
        public string tenantID;
    }

    public class S2SAuthenticationResponse
    {
        public string token_type;
        public string expires_in;
        public string ext_expires_in;
        public string access_token;
    }
}
