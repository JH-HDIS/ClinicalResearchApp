using System;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

public class OAuthHelper
{
    private readonly string _clientId;
    private readonly string _tenantId;
    private readonly string _clientSecret;

    public OAuthHelper(string clientId, string tenantId, string clientSecret)
    {
        _clientId = clientId;
        _tenantId = tenantId;
        _clientSecret = clientSecret;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(_clientId)
            .WithClientSecret(_clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{_tenantId}")
            .Build();

        var result = await app.AcquireTokenForClient(new[] { "https://outlook.office365.com/.default" }).ExecuteAsync();
        return result.AccessToken;
    }
}
