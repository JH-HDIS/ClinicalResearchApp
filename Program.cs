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
    .WriteTo.File("Logs/myapp.log", rollingInterval: RollingInterval.Day)  // Log to a file with daily rolling
    .CreateLogger();

builder.Host.UseSerilog();  // Integrate Serilog with the ASP.NET Core logging system

// Bypass certificate - temporary for testing only
ServicePointManager.ServerCertificateValidationCallback = 
        (sender, certificate, chain, sslPolicyErrors) => true;

// Bind OpenIdConnect configuration from appsettings.json
var openIdConnectSettings = builder.Configuration.GetSection("OpenIdConnect");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // Allows cross-origin cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS
});

// Register EmailService for Dependency Injection
builder.Services.AddSingleton<EmailService>();

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Configure authentication
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

    // Use JWT Bearer for token validation
    options.Events = new OpenIdConnectEvents
    {
        OnAuthorizationCodeReceived = context =>
        {
            Console.WriteLine($"Authorization code received: {context.ProtocolMessage.Code}");
            Log.Information("OnAuthorizationCodeReceived Event!! ProtocolMessage Code: ", context.ProtocolMessage?.Code);
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Authorization code received:");
            logger.LogDebug(context.ProtocolMessage?.Code);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Log.Information("OnMessageReceived Event!! ProtocolMessage: ", context.ProtocolMessage?.ToString());
            Console.WriteLine(context.ProtocolMessage?.ToString());
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Message received from Identity Provider:");
            logger.LogDebug(context.ProtocolMessage?.ToString());
            return Task.CompletedTask;
        },
        OnRedirectToIdentityProvider = context =>
        {
            Log.Information("OnRedirectToIdentity Provider Event!! Redirecting to Identity Provider. Request: {RequestUri}", context.ProtocolMessage.CreateAuthenticationRequestUrl());

            // Bypass certificate validation for testing only
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            context.Options.BackchannelHttpHandler = handler;
            var requestURI = context.ProtocolMessage.RequestUri;
            context.HttpContext.Items["RequestURI"] = requestURI;

            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            Log.Error("OnRemoteFailure Event! Remote Failure in OIDC: {ErrorMessage}. Exception: {Exception}",
                context.Failure?.Message,
                context.Failure);
            context.Response.Redirect("/Home/Error");
            context.HandleResponse(); // Suppress default error handling
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Log.Error("OnAuthenticationFailed Event! Remote Failure in OIDC: {ErrorMessage}. Exception: {Exception}",
                context.Exception?.Message,
                context.Exception?.StackTrace);
            var errorMessage = context.Exception?.Message ?? "Unknown error";
            var errorDetails = context.Exception?.StackTrace ?? "No Exception stack trace available";
            var contextMsg = context.ProtocolMessage?.ToString() ?? "No ProtocolMessage available";
            var requestURI = context.ProtocolMessage?.RequestUri;
            context.HttpContext.Items["RequestURI"] = requestURI;
            context.HttpContext.Items["ErrorMessage"] = errorMessage;
            context.HttpContext.Items["ErrorDetails"] = errorDetails + contextMsg + requestURI;
            context.Response.Redirect("/Home/Error");
            context.HandleResponse(); // Suppress default error handling
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var idToken = context.SecurityToken as JwtSecurityToken;
            Console.WriteLine($"ID Token: {idToken.RawData}");
            Log.Information("OnTokenValidated Event!!", idToken.ToString);
            foreach (var claim in idToken.Claims)
            {
                Log.Information($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
