using PowerBI.ReportingServer.Extensions.Options;
using PowerBI.ReportingServer.Extensions.Services;
using System;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Security;

namespace PowerBI.ReportingServer.Extensions
{
   public class Sso: System.Web.UI.Page
   {
        private OAuthOption oauthConfiguration;
        private OAuthService oauthService;

        override protected void OnInit(EventArgs e)
        {
            var optionService = new OptionService();
            oauthConfiguration = optionService.ConfigurationOption;
            this.oauthService = new OAuthService(oauthConfiguration, GetRedirectUri(HttpContext.Current.Request));

            InitializeComponent();
            base.OnInit(e);
          
        }

        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }
        //#endregion

        private void Page_Load(object sender, System.EventArgs e)
        {
            var context = HttpContext.Current;
            if (context.User != null
             && context.User.Identity != null
             && context.User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(FormsAuthentication.GetRedirectUrl(context.User.Identity.Name, false)))
                {
                    context.Response.Redirect("/reports");
                }
                return;
            }


            var currentRequest = context.Request;
            if (currentRequest.HttpMethod.Equals("get", StringComparison.OrdinalIgnoreCase))
            {
                var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(currentRequest.QueryString["ReturnUrl"]));

                // Construct the final URL
                string finalUrl = this.oauthService.getOAuthUrl(state);

                // Redirect the user to the OIDC provider login page
                context.Response.Redirect(finalUrl, true);
                return;
            }

            if ((currentRequest.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase)))
            {
                string authorizationCode = Request.Form["code"];
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    // Exchange the authorization code for tokens at the OIDC token endpoint
                    string idToken = this.oauthService.ExchangeAuthorizationCodeForTokens(authorizationCode).Result;

                    if (!string.IsNullOrEmpty(idToken))
                    {

                        ClaimsPrincipal claimsPrincipal = this.oauthService.ValidateToken(idToken);
                        var email = this.oauthService.GetUserEmailOrUsername(claimsPrincipal);
                        var origUrl = Encoding.UTF8.GetString(Convert.FromBase64String(context.Request.Form["state"]));
                        FormsAuthentication.SetAuthCookie(email, false);
                        context.Response.Redirect(origUrl);
                        return;
                    }
                }
            }
        }

        private string GetRedirectUri(HttpRequest request)
        {
            var url = request.Url;
            var redirectUrl = $"{url.Scheme}://{url.Host}/reportserver/sso.aspx";
            return redirectUrl;
        }
    }
}
