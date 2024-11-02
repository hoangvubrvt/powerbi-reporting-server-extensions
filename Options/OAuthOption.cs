
namespace PowerBI.ReportingServer.Extensions.Options
{
    public sealed class OAuthOption
    {

        public string BaseUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string AuthroizationEndpoint { get { return $"{this.BaseUrl}/oauth2/v2.0/authorize"; } }

        public string OpenIDConfigurationEndpoint { get { return $"{this.BaseUrl}/v2.0/.well-known/openid-configuration"; } }

        public string TokenEndpoint { get { return $"{this.BaseUrl}/oauth2/v2.0/token"; } }

        public string IssuerUrl { get { return $"{this.BaseUrl}/v2.0"; } }

    }
}
