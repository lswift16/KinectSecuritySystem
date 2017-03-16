using System;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace EmailTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("What is your email: ");
            var email = Console.ReadLine();

            string currentTime = DateTime.Now.ToString("dd/MM/yy h:mm:ss tt");

            /*
            //Set the attachment path to that of the application
            string attachmentPath = "Insert file path here!";

            //Get the attachment data.xml found at this path
            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachmentPath + "/KinectPhoto.filetype");
            */

            //Make a new email message
            MailMessage mail = new MailMessage();

            //Set the various variables required for an email
            mail.From = new MailAddress("beauzstantonfyp@gmail.com");
            mail.To.Add("beauzstantonfyp@gmail.com");
            mail.Subject = "KinectSecurity Alert";
            mail.Body = "A user has attempted to unlock your KinectSecurity System at: " + currentTime;
            //mail.Attachments.Add(attachment);

            //Create a new emailServer and set its details including email & password
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("beauzstantonfyp", "b3018570") as ICredentialsByHost;
            smtpServer.EnableSsl = true;
            
            //Check the security certificates and return applicable errors (if any exist)
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
                { return true; };

            //Send the email
            smtpServer.Send(mail);
        }
    }
}
