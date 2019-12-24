using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Simple.Authentication
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		// Don't do this in production...
		private readonly string UserName;
		private readonly string Password;

		public BasicAuthenticationHandler(
			IServiceProvider serviceProvider,
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
			ServiceProvider = serviceProvider;
			UserName = ServiceProvider.GetService<IConfiguration>().GetValue<string>("UserName");
			Password = ServiceProvider.GetService<IConfiguration>().GetValue<string>("Password");
		}
		public IServiceProvider ServiceProvider { get; }

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
				new Claim(ClaimTypes.NameIdentifier, UserName),
				new Claim(ClaimTypes.Name, UserName),
			};

			return Task.FromResult(AuthenticateResult.Success(
				new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
		}

		private bool Authenticate(string username, string password) => username == UserName && password == Password;
	}
}