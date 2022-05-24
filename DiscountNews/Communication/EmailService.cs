using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DiscountNews.Communication
{
    public class EmailService:IEmailService
    {
        public void SendEmail(string email,string productName,string url)
        {
            try
            {
                string fromaddr = "yourEmail";
                string password = "yourPassword";

                MailMessage msg = new MailMessage();
                msg.Subject = $"TRENDYOL - THE DISCOUNT YOU WERE EXPECTED ON YOUR {productName} AT TRENDYOL!";
                msg.From = new MailAddress(fromaddr);
                msg.Body = $"<p> The product {productName} has fallen below the price you specified. </p> </br>" +
                    $" <p><a href='{url}'>Click here<a/> to access the product.</p>";
                msg.To.Add(new MailAddress(email));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                msg.IsBodyHtml = true;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential(fromaddr, password);
                smtp.Credentials = nc;
                smtp.Send(msg);
            }

            catch (SmtpException ex)
            {
                throw new ApplicationException
                  ("SmtpException has occured: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
