using System;

namespace dashboard.Models
{
    public class Billing
    {
        public DateTime Date { get; set; }
        public string SubscriptionId { get; set; }
        public double Value { get; set; }
    }
}
