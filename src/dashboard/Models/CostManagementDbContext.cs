using services.Utilities;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace dashboard.Models
{
    public class CostManagementDbContext
    {
        public List<Billing> GetWeeklyBilling()
        {
            List<Billing> list = new List<Billing>();

            try
            {
                using SqlConnection conn = new SqlConnection(Utils.DbConnectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand("select date, subscriptionId, value from billing", conn);

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Billing()
                    {
                        Date = reader.GetDateTime(0),
                        SubscriptionId = reader.GetString(1),
                        Value = reader.GetDouble(2)
                    });
                }
                return list;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}