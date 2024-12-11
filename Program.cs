using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/myapp.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Bypass certificate validation - temporary for testing only
ServicePointManager.ServerCertificateValidationCallback =
    (sender, certificate, chain, sslPolicyErrors) => true;

// Determine if running on localhost
var isLocalhost = builder.Environment.IsDevelopment() ||
                  builder.Configuration["ASPNETCORE_URLS"]?.Contains("localhost") == true;

if (isLocalhost)
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Home/LocalLogin"; // Point to a local test login page
        });
    builder.Services.AddAuthorization();
    builder.Services.AddControllersWithViews();
}
else
{
    var openIdConnectSettings = builder.Configuration.GetSection("OpenIdConnect");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = openIdConnectSettings["Authority"];
        options.ClientId = openIdConnectSettings["ClientId"];
        options.ClientSecret = openIdConnectSettings["ClientSecret"];
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.UsePkce = false;
        options.ClaimActions.MapJsonKey("uid", "uid");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://login.jh.edu/idp/shibboleth"
        };

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.CallbackPath = openIdConnectSettings["CallbackPath"];

        options.Events = new OpenIdConnectEvents
        {
            // Existing event handlers
        };
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
