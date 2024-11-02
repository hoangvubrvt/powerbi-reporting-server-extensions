using PowerBI.ReportingServer.Extensions.Options;
using System.Web.Configuration;

namespace PowerBI.ReportingServer.Extensions.Services
{
    public class OptionService
    {
        private OAuthOption oauthConfiguration;
        public OptionService() {
            oauthConfiguration = new OAuthOption();

            var baseUrl = WebConfigurationManager.AppSettings.Get("oauth.baseurl");
            var tenantId = WebConfigurationManager.AppSettings.Get("oauth.tenant.id");
            var clientId = WebConfigurationManager.AppSettings.Get("oauth.client.id");
            var clientSecret = WebConfigurationManager.AppSettings.Get("oauth.client.secret");

            oauthConfiguration.BaseUrl = $"{baseUrl}/{tenantId}";
            oauthConfiguration.ClientId = clientId;
            oauthConfiguration.ClientSecret = clientSecret;
        }

        public OAuthOption ConfigurationOption { get { return oauthConfiguration; } }
    }
}
