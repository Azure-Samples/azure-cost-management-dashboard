using services.Database;
using System.Data;
using System.Data.SqlClient;

namespace services.APIs.CostManagement
{
    public class CostManagementDataService
    {
        const string commandSql = "select top 8 sum(value) as total, date from[dbo].[billing] group by date order by date";
        const int columnTotal = 0;
        const int columnDate = 1;
        public async Task<List<WeeklyBillingDto>> GetWeeklyBillingAsync()
        {
            var list = new List<WeeklyBillingDto>();

            try
            {
                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();

                var cmd = new SqlCommand(commandSql, connection);

                using SqlDataReader reader = cmd.ExecuteReader();

                while (await reader.ReadAsync())
                {
                    list.Add(new WeeklyBillingDto()
                    {
                        Total = reader.GetDouble(columnTotal),
                        Date = reader.GetDateTime(columnDate),
                    });
                }
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BillingDto> GetLatestBillingForTodayAsync(string subscriptionId)
        {

            if (string.IsNullOrEmpty(subscriptionId))
                return null;

            try
            {
                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();

                SqlCommand cmd = new SqlCommand("select date, subscriptionId, value from billing where subscriptionId = @subId and date = @today", connection);
                cmd.Parameters.Add("@subId", SqlDbType.VarChar).Value = subscriptionId;
                cmd.Parameters.Add("@today", SqlDbType.Date).Value = DateTime.Now.Date;

                using SqlDataReader reader = cmd.ExecuteReader();

                var data = new BillingDto();

                while (await reader.ReadAsync())
                {
                    data.Date = reader.GetDateTime(0);
                    data.SubscriptionId = reader.GetString(1);
                    data.Value = reader.GetDouble(2);
                }
                return data;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<MonthToDateDto>> GetMonthToDateBillingAsync()
        {
            List<MonthToDateDto> list = new List<MonthToDateDto>();

            try
            {
                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();

                SqlCommand cmd = new SqlCommand("select subscriptionId, Value, DATENAME(month, getdate()) as Month from [dbo].[billing] where date = @today", connection);
                cmd.Parameters.Add("@today", SqlDbType.Date).Value = DateTime.Now.Date;

                using SqlDataReader reader = cmd.ExecuteReader();

                while (await reader.ReadAsync())
                {
                    list.Add(new MonthToDateDto()
                    {
                        SubscriptionId = reader.GetString(0),
                        Value = reader.GetDouble(1),
                        Month = reader.GetString(2)
                    });
                }
                return list;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task SaveBillingAsync(BillingDto dto)
        {
            try
            {

                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();

                var sql = string.Empty;
                if (!dto.IsUpdate)
                    sql = "insert into [dbo].[billing] values(@date, @subscriptionId, @value, getdate());";
                else
                    sql = "update [dbo].[billing] set value = @value, lastupdated = getdate() where date = @date and subscriptionId = @subscriptionId;";


                if (dto.PercentChanged >= Utils.PercentageWarning)
                {
                    sql += " insert into [dbo].[billinglog] values (NEWID(), getdate(), @subscriptionId, @value, @valuechangepercent, 0);";
                }

                using SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.Add("@date", SqlDbType.Date).Value = dto.Date;
                cmd.Parameters.Add("@subscriptionId", SqlDbType.VarChar, 50).Value = dto.SubscriptionId;
                cmd.Parameters.Add("@value", SqlDbType.Float).Value = dto.Value;
                cmd.Parameters.Add("@valuechangepercent", SqlDbType.Float).Value = dto.PercentChanged;

                cmd.CommandType = CommandType.Text;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        const string QueryConsumption = "SELECT id, date, subscriptionId, value, valuechangepercent, emailsent FROM billinglog WHERE emailsent = 0";
        public async ValueTask<List<BillingLogDto>> NotifyConsumptionIncreaseByEmailAsync()
        {
            var list = new List<BillingLogDto>(10);

            try
            {
                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();

                var cmd = new SqlCommand(QueryConsumption, connection);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new BillingLogDto()
                    {
                        Id = reader.GetGuid(0),
                        Date = reader.GetDateTime(1),
                        SubscriptionId = reader.GetString(2),
                        Value = reader.GetDouble(3),
                        ValueChangePercent = reader.GetDouble(4),
                        IsEmailSent = reader.GetBoolean(5)
                    });
                }

                await connection.CloseAsync();

                return list;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        const string StatementUpdate = "UPDATE [billinglog] SET emailsent = 1 WHERE id = @id;";

        public async ValueTask UpdateEmailNotificationAsync(Guid id)
        {
            try
            {

                using var connection = DatabaseFactory.GetConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = StatementUpdate;
                command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;

                command.CommandType = CommandType.Text;
                await command.ExecuteNonQueryAsync();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
