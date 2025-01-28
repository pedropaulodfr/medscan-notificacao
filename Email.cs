using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace medscanner_notificacao
{
    internal class Email
    {
        public static void EnviarEmail(string email, string titulo, string body)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Configurações do SMTP
            string smtpHost = string.Empty;
            string smtpPort = string.Empty;
            string smtpUser = string.Empty;
            string smtpSenha = string.Empty;

            string querySetupSMTP = "SELECT smtpHost, smtpPort, smtpUser, smtpPassword, URLWeb FROM Setup";
            DataTable SMTPTable = DatabaseHelper.GetDataTable(querySetupSMTP);

            if (SMTPTable.Rows.Count > 0)
            {
                smtpHost = SMTPTable.Rows[0]["smtpHost"].ToString();
                smtpPort = SMTPTable.Rows[0]["smtpPort"].ToString();
                smtpUser = SMTPTable.Rows[0]["smtpUser"].ToString();
                smtpSenha = SMTPTable.Rows[0]["smtpPassword"].ToString();
                body = body.Replace("{URLWeb}", SMTPTable.Rows[0]["URLWeb"].ToString());
            }

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(smtpUser);
                    mail.To.Add(email);
                    mail.Subject = titulo;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(smtpHost, int.Parse(smtpPort)))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpSenha);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                Console.WriteLine($"E-mail enviado para {email}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail para {email}: {ex.Message}");
            }
        }
    }
}
