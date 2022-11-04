namespace services.APIs.CostManagement;

public static class CostManagementService
{
    private static HttpClient _httpClient = null;
    private readonly static SemaphoreSlim _semaphoreSlim = new(1, 1);
    private const int elementBillingValue = 0;

    public static async Task AzureBillingMonthToDateApiFetchAsync()
    {
        if (string.IsNullOrEmpty(Utils.subscriptionIdList))
            throw new Exception(Constants.SubscriptionEmpty);

        var subscriptions = Utils.subscriptionIdList.Split(',').ToList();

        if (_httpClient == null)
        {
            await _semaphoreSlim
                    .WaitAsync()
                    .ConfigureAwait(false);

            _httpClient = new HttpClient();

            _semaphoreSlim.Release();
        }

        var jsonContent = new StringContent(GetBillingMonthToDateJson(),
                                            Encoding.UTF8,
                                            "application/json");

        var token = await AzureIdentityService.GetToken(new (Utils.TenantId, Utils.ClientId, Utils.ClientSecret));

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        try
        {
            foreach (var subscription in subscriptions)
            {
                var url = $"https://management.azure.com/subscriptions/{subscription}/providers/Microsoft.CostManagement/query?api-version=2021-10-01";
                
                var result = await _httpClient
                                    .PostAsync(url, jsonContent)
                                    .ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                var jsonData = await result
                                        .Content
                                        .ReadFromJsonAsync<JsonElement>();

                var row = jsonData
                            .GetProperty("properties")
                            .GetProperty("rows");

                if (row.ToString() != "[]")
                {
                    var costManagementService = new CostManagementDataService();
                    var rows = row.EnumerateArray();
                    var billingValue = rows
                                        .First()
                                        .EnumerateArray()
                                        .ElementAt(elementBillingValue)
                                        .GetDouble();

                    var billing = new BillingDto(subscription, billingValue);

                    //Check if there is already a cost for the day, if there is, update it, otherwise insert a new value
                    var todaysValue = await costManagementService
                                            .GetLatestBillingForTodayAsync(subscription)
                                            .ConfigureAwait(false);

                    if (todaysValue.SubscriptionId != null)
                    {
                        billing.CalculatePercent(todaysValue.Value);
                    }

                    //Save to SQL Database
                    await costManagementService
                            .SaveBillingAsync(billing)
                            .ConfigureAwait(false);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    internal static string GetBillingMonthToDateJson()
    {
        return "{ \"type\": \"Usage\", \"timeframe\": \"BillingMonthToDate\", \"dataset\": { \"granularity\": \"None\", \"aggregation\": { \"totalCost\": { \"name\": \"PreTaxCost\", \"function\": \"Sum\"}  } } }";
    }

    public static async Task<List<WeeklyBillingDto>> GetWeeklyBillingAsync()
    {
        var service = new CostManagementDataService();
        return await service.GetWeeklyBillingAsync();
    }

    public static async Task<List<MonthToDateDto>> GetMonthToDateBillingAsync()
    {
        var service = new CostManagementDataService();
        return await service.GetMonthToDateBillingAsync();
    }

    public static async ValueTask NotifyConsumptionIncreaseByEmailAsync()
    {
        var service = new CostManagementDataService();
        var notifications = await service.NotifyConsumptionIncreaseByEmailAsync();

        foreach (var item in notifications)
        {
            if (!item.IsEmailSent && item.ValueChangePercent >= Utils.PercentageWarning)
            {

                await new EmailService().SendEmailAsync(item.SubscriptionId, item.ValueChangePercent.ToString());
                await new CostManagementDataService().UpdateEmailNotificationAsync(item.Id);
            }
        }

    }

}
