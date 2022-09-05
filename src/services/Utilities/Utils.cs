namespace services.Utilities
{
    public static class Utils
    {
        public const string TenantId = "tenant_id_azure_ad"; //Tenant ID Azure AD
        public const string ClientId = "application_id_in_azure_ad"; //Application ID in Azure AD
        public const string ClientSecret = "app_secret_value"; // App's secret Value (not Secret ID)
        public const string subscriptionIdList = "your_subscription_divided_by_command"; //Ex: xyz-xpto,abc-def

        public const string DbConnectionString = "your_sql_server_datasource";
        public const int PercentageWarning = 10;

        public const string EmailFrom = "email_from";
        public const string EmailTo = "email_to";
        public const string EmailHost = "email_host";
        public const int EmailPort = 587;
        public const string EmailCredential = "email";
        public const string EmailPassword = "password";

        public const int RunCostManagemementApiInSeconds = 3600; //seconds to run the job
        public const int RunEmailRescheduleInSeconds = 3600; //seconds to run the job
    }
}