using Microsoft.IdentityModel.Protocols;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Data;
using medscanner_notificacao;


class Program
{
    static void Main(string[] args)
    {
        // String de conexão com o banco de dados
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // Query para buscar usuários que devem receber o e-mail
        string queryUsuarios = string.Format(@"SELECT
                                        CONVERT(DATE, CC.DataRetorno, 103) DataRetorno,
                                        M.Identificacao  + ' ' + M.Concentracao + ' ' + U.Identificacao Medicamento,
                                        P.Nome Nome,
                                        P.Email
                                        FROM CartaoControle CC 
                                        JOIN Medicamentos M on M.Id = CC.Medicamento_Id
                                        JOIN Pacientes P on P.Id = CC.Paciente_Id
                                        JOIN Unidades U on U.Id = M.Unidade_Id
                                        WHERE DATEDIFF(DAY, GETDATE(), CC.DataRetorno) BETWEEN 0 AND 15");
        
        string queryEmailTemplate = "SELECT [Titulo], [Corpo] FROM Emails WHERE Identificacao = 'NotificacaoRetorno' AND Ativo = 1";

        try
        {
            DataTable emailTemplateTable = DatabaseHelper.GetDataTable(queryEmailTemplate, connectionString);
            DataTable usuariosTable = DatabaseHelper.GetDataTable(queryUsuarios, connectionString);

            string titulo = string.Empty;
            string corpo = string.Empty;

            if(emailTemplateTable.Rows.Count > 0)
            {
                titulo = emailTemplateTable.Rows[0]["Titulo"].ToString();
                corpo = emailTemplateTable.Rows[0]["Corpo"].ToString();
            }

            foreach (DataRow row in usuariosTable.Rows)
            {
                string nome = row["Nome"].ToString();
                string email = row["Email"].ToString(); 
                string medicamento = row["Medicamento"].ToString();
                string dataRetorno = row["DataRetorno"].ToString();

                string body = corpo.Replace("{NOME}", nome)
                                    .Replace("{MEDICAMENTO}", medicamento)
                                    .Replace("{DATARETORNO}", dataRetorno)
                                    .Replace("{ICONEMEDICAMENTO}", "💊")
                                    .Replace("{ICONECALENDARIO}", "📆");

                Email.EnviarEmail(email, titulo, body);
            }

            Console.WriteLine("E-mails enviados com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }
}
