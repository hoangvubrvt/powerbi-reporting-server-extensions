using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PowerBI.ReportingServer.Extensions.Options;

namespace PowerBI.ReportingServer.Extensions.Services
{
    public class OAuthService
    {
        private OAuthOption oauthConfiguration;
        private string redirectUri;
        public OAuthService(OAuthOption oauthConfiguration, string redirectUri) 
        { 
            this.oauthConfiguration = oauthConfiguration;
            this.redirectUri = redirectUri;
        }

        public string getOAuthUrl(string state)
        {
            var parameters = new Dictionary<string, string>
                {
                    { "client_id", this.oauthConfiguration.ClientId },
                    { "redirect_uri", this.redirectUri},
                    { "response_type", "code" },
                    { "response_mode", "form_post" },
                    { "state", state },
                    { "scope", "openid profile email" } // Space-separated scope needs encoding
                };

            // Encode the parameters
            string queryString = EncodeParameters(parameters);

            // Construct the final URL
            string finalUrl = $"{this.oauthConfiguration.AuthroizationEndpoint}?{queryString}";

            return finalUrl;
        }

        public ClaimsPrincipal ValidateToken(string idToken)
        {
            // Validate the token and extract user info
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(idToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = this.oauthConfiguration.IssuerUrl,  // Uses the issuer from the discovery document
                ValidateAudience = true,
                ValidAudience = this.oauthConfiguration.ClientId,         // Your app's Client ID
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = GetSigningKeys(),  // Uses signing keys from the discovery document
                ValidateLifetime = true
            }, out _);

            return claimsPrincipal;
        }

        public string GetUserEmailOrUsername(ClaimsPrincipal claimsPrincipal) 
        {
            return claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "email")?.Value.ToLower() ?? claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value.ToLower();
        }
        
        private string EncodeParameters(Dictionary<string, string> parameters)
        {
            List<string> encodedParams = new List<string>();

            foreach (var param in parameters)
            {
                string encodedKey = HttpUtility.UrlEncode(param.Key);
                string encodedValue = HttpUtility.UrlEncode(param.Value);

                encodedParams.Add($"{encodedKey}={encodedValue}");
            }

            // Join all the encoded parameters with '&'
            return string.Join("&", encodedParams);
        }

        private IEnumerable<SecurityKey> GetSigningKeys()
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                this.oauthConfiguration.OpenIDConfigurationEndpoint,
                new OpenIdConnectConfigurationRetriever());

            var config = configManager.GetConfigurationAsync().Result;
            return config.SigningKeys;
        }

        public async Task<string> ExchangeAuthorizationCodeForTokens(string code)
        {
            // Call OIDC provider's token endpoint to exchange the code for tokens
            using (HttpClient client = new HttpClient())
            {
                var requestData = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", this.oauthConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret", this.oauthConfiguration.ClientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", this.redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("scope", "openid profile email")
            });

                HttpResponseMessage response = await client.PostAsync(this.oauthConfiguration.TokenEndpoint, requestData);
                string responseBody = await response.Content.ReadAsStringAsync();

                var tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                return tokenResult?.IdToken;
            }
        }

        private class TokenResponse
        {
            [Newtonsoft.Json.JsonProperty("id_token")]
            public string IdToken { get; set; }
        }
    }
}
