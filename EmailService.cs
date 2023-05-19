using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace ForumSite
{
    interface IEmailService
    {
        bool SendEmailCode(string to_email, string code);
        string GenerateCode();
        bool isValidEmail(string email);
    }

    public class EmailService: IEmailService
    {
        public bool SendEmailCode(string to_email, string code)
        {
            string from_email = "gnusarovvladislav@gmail.com";
            String password = "jamedkhdwsglodox";

            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from_email, password);
                try
                {
                    using (var message = new MailMessage(
                        from: new MailAddress(from_email, "Сайт Forum \"About Everything\""),
                        to: new MailAddress(to_email)
                        ))
                    {
                        message.Subject = "Подтверждение электронной почты";
                        message.Body = $"Ваш код: {code}";
                        client.Send(message);

                        return true;
                    }
                }
                catch (FormatException ex)
                {
                    return false;
                }

            }
        }

        public string GenerateCode()
        {
            Random rnd = new Random();

            int code = rnd.Next(10000, 99999);

            return code.ToString();
        }

        public bool isValidEmail(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
    }
}
