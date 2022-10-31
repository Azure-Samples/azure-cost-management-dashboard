namespace services.Dtos;

public record BillingDto
{
    private const double Zero = 0;
    private const int percentCalc = 100;
    private const int roundBillingValue = 2;

    public BillingDto(){}

    public BillingDto(string subscription,
                        double value)
    {
        SubscriptionId = subscription;
        Date = DateTime.UtcNow.Date;
        IsUpdate = false;
        PercentChanged = Zero;
        Value = Math.Round(value, roundBillingValue);  
    }

    public DateTime Date { get; set; }
    public string SubscriptionId { get; set; }
    public double Value { get; set; }
    public bool IsUpdate { get; set; }
    public double PercentChanged { get; set; }

    public void CalculatePercent(double todaysValue)
    {
        IsUpdate = true;
        //find percentual of increase between last value and current value
        if (Value > Zero)
        {
            var changedValue = (todaysValue - Value) / Value * percentCalc;
            PercentChanged = Math.Round(changedValue, roundBillingValue);
        }
    }
}

public record WeeklyBillingDto
{
    public double Total { get; set; }
    public DateTime Date { get; set; }
}

public record MonthToDateDto
{
    public string SubscriptionId { get; set; }
    public double Value { get; set; }
    public string Month { get; set; }
}

public record DashboardViewModel
{
    public IEnumerable<WeeklyBillingDto> WeeklyBillingList { get; set; }
    public IEnumerable<MonthToDateDto> MonthToDateDtoList { get; set; }
}

public record BillingLogDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string SubscriptionId { get; set; }
    public double Value { get; set; }
    public double ValueChangePercent { get; set; }
    public bool IsEmailSent { get; set; }
}
