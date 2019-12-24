using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Simple.Authentication;
using Simple.Endpoints;
using Simple.Extensions;
using Simple.Services;

using tusdotnet;
using tusdotnet.Models;

namespace Simple
{
	public class Startup
	{
		public Startup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(CreateTusConfiguration);

			services.AddHostedService<ExpiredFilesCleanupService>();

			services.AddAuthentication("BasicAuthentication")
					.AddScheme<AuthenticationSchemeOptions,
					BasicAuthenticationHandler>("BasicAuthentication", null)
			;
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.Use((context, next) =>
			{
				context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
				return next.Invoke();
			});

			if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

			app.UseSimpleExceptionHandler();

			app.UseAuthentication();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			app.UseHttpsRedirection();

			app.UseTus(httpContext => Task.FromResult(httpContext.RequestServices.GetService<DefaultTusConfiguration>()));

			app.UseRouting();

			app.UseEndpoints(endpoints => endpoints.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute));
		}

	}
}
