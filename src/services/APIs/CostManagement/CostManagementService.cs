namespace services.APIs.CostManagement;

public static class CostManagementService
{
    private static HttpClient _httpClient = null;
    private readonly static SemaphoreSlim _semaphoreSlim = new(1, 1);

    public static async Task AzureBillingMonthToDateApiFetch()
    {
        if (string.IsNullOrEmpty(Utils.subscriptionIdList))
            throw new Exception(Constants.SubscriptionEmpty);

        var subscriptionList = Utils.subscriptionIdList.Split(',').ToList();

        if (_httpClient == null)
        {
            await _semaphoreSlim.WaitAsync();
            _httpClient = new HttpClient();
            _semaphoreSlim.Release();
        }

        var jsonContent = new StringContent(
            GetBillingMonthToDateJson(),
            Encoding.UTF8,
            "application/json");

        var token = await AzureIdentityService.GetToken(new (Utils.TenantId, Utils.ClientId, Utils.ClientSecret));

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        try
        {
            foreach (var sub in subscriptionList)
            {
                var url = "https://management.azure.com/subscriptions/" + sub + "/providers/Microsoft.CostManagement/query?api-version=2021-10-01";
                var result = await _httpClient.PostAsync(url, jsonContent);

                result.EnsureSuccessStatusCode();

                var jsonData = await result.Content.ReadFromJsonAsync<JsonElement>();

                var row = jsonData.GetProperty("properties").GetProperty("rows");

                if (row.ToString() != "[]")
                {
                    var rows = row.EnumerateArray();

                    var billing = new BillingDto
                    {
                        Date = DateTime.UtcNow.Date,
                        SubscriptionId = sub,
                        Value = Math.Round(rows.First().EnumerateArray().ElementAt(0).GetDouble(), 2),
                        IsUpdate = false,
                        PercentChanged = 0
                    };

                    var service = new CostManagementDataService();

                    //Check if there is already a cost for the day, if there is, update it, otherwise insert a new value
                    var todaysValue = await service.GetLatestBillingForTodayAsync(sub);
                    if (todaysValue.SubscriptionId != null)
                    {
                        billing.Value = billing.Value;
                        billing.IsUpdate = true;
                        //find percentual of increase between last value and current value
                        billing.PercentChanged = (billing.Value > 0 ? Math.Round(((todaysValue.Value - billing.Value) / billing.Value) * 100, 2) : 0);
                    }

                    //Save to SQL Database
                    await service.SaveBillingAsync(billing);
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
