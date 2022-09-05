namespace services.Token;

public record AzureIdentityTokenRequestModel(string TenantId, string ClientId, string ClientSecret);

public static class AzureIdentityService
{
    public static async Task<string> GetToken(AzureIdentityTokenRequestModel model)
    {
        var credentials = new ClientSecretCredential(model.TenantId, model.ClientId, model.ClientSecret);
        var token = await credentials.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }, tenantId: model.TenantId)
            );
        return token.Token;
    }
}
