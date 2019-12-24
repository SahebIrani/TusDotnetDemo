using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Simple.Authentication
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		// Don't do this in production...
		private const string Username = "Sinjul";
		private const string Password = "MSBH";

		public BasicAuthenticationHandler(
			IConfiguration configuration,
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!Request.Headers.ContainsKey("Authorization"))
				return Task.FromResult(AuthenticateResult.NoResult());

			bool isAuthenticated;
			try
			{
				AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
				string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
				isAuthenticated = Authenticate(credentials[0], credentials[1]);
			}
			catch
			{
				return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
			}

			if (!isAuthenticated)
				return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));



			var claims = new[] {
				new Claim(ClaimTypes.NameIdentifier, Username),
				new Claim(ClaimTypes.Name, Username),
			};

			return Task.FromResult(AuthenticateResult.Success(
				new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
		}

		private bool Authenticate(string username, string password) => username == Username && password == Password;
	}
}