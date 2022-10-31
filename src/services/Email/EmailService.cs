using System.Net.Mail;

namespace services.Email;

public class EmailService
{
    public async ValueTask SendEmailAsync(string subscriptionId, string percent)
    {
        try
        {

            var isValidRequest =
                !string.IsNullOrEmpty(Utils.EmailFrom) &&
                !string.IsNullOrEmpty(Utils.EmailTo) &&
                !string.IsNullOrEmpty(Utils.EmailHost) &&
                !string.IsNullOrEmpty(Utils.EmailCredential) &&
                !string.IsNullOrEmpty(Utils.EmailPassword) &&
                Utils.EmailPort > 0;

            if (!isValidRequest)
                return;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(Utils.EmailFrom)
            };

            mailMessage.To.Add(new MailAddress(Utils.EmailTo));
            mailMessage.Subject = "Email de Notificação de Consumo Azure";

            mailMessage.IsBodyHtml = true;
            mailMessage.Body = GetEmailBody(subscriptionId, percent);

            SmtpClient sMTPClient = new SmtpClient
            {
                EnableSsl = true,
                Host = Utils.EmailHost,
                Port = Utils.EmailPort,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(Utils.EmailCredential, Utils.EmailPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await sMTPClient
                    .SendMailAsync(mailMessage)
                    .ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            throw;
        }

    }

    internal static string GetEmailBody(string subscriptionId, string percent)
    {
        var emailBody = "<!DOCTYPE html><html lang='en' xmlns='http://www.w3.org/1999/xhtml'><head> <meta charset='utf-8' /> <style> /* DivTable.com */.divTable{display: table;width: 50%; text-align: center;}.divTableRow {display: table-row;}.divTableHeading {background-color: #EEE;display: table-header-group;}.divTableCell, .divTableHead {border: 1px solid #999999;display: table-cell;padding: 3px 10px;}.divTableHeading {background-color: #EEE;display: table-header-group;font-weight: bold;}.divTableFoot {background-color: #EEE;display: table-footer-group;font-weight: bold;}.divTableBody {display: table-row-group;}body{ font-family: 'Montserrat', sans-serif;}</style><link rel='preconnect' href='https://fonts.googleapis.com'><link rel='preconnect' href='https://fonts.gstatic.com' crossorigin><link href='https://fonts.googleapis.com/css2?family=Montserrat:wght@300&display=swap' rel='stylesheet'> <title></title></head><body> <div class='divTable'> <div class='divTableBody'> <div class='divTableRow'> <div class='divTableCell'> <img src='https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE1Mu3b?ver=5c31' /> <h1>Azure Consumption Notification</h1> <p>Você está recebendo esse e-mail devido a um acréscimo de consumo de " + percent + "% na sua subscrição " + subscriptionId + " !</p> <p> <a href='https://portal.azure.com/'>Analisar via Portal Azure</a> </p> </div> </div> </div> </div> <!-- DivTable.com --></body></html>";
        return emailBody;
    }

}
