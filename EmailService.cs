using System.Net.Mail;
using System.Net;

namespace ForumSite
{
    public class EmailService
    {
        private string fromMail;
        private string nameFromMail;
        private string password = "semudlymuhxanbky";

        public EmailService(string fromMail = "gnusarovvladislav@gmail.com", string nameFromMail = "Код подтверждения") 
        {
            this.fromMail = fromMail;
            this.nameFromMail = nameFromMail; 
        }

        public void SendMessage(string toMail, string toName, string subject, string text)
        {
            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(this.fromMail, this.password);
                using (var message = new MailMessage(
                    from: new MailAddress(this.fromMail, this.nameFromMail),
                    to: new MailAddress(toMail, toName)
                    ))
                {

                    message.Subject = subject;
                    message.Body = text;

                    client.Send(message);
                }
            }
        }

    }
}
