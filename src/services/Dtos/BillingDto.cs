namespace services.Dtos;

public record BillingDto
{
    public DateTime Date { get; set; }
    public string SubscriptionId { get; set; }
    public double Value { get; set; }
    public bool IsUpdate { get; set; }

    public double PercentChanged { get; set; }
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
