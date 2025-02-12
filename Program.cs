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
        // Inicia o servidor HTTP em uma tarefa separada
        var hostTask = CreateHostBuilder(args).Build().RunAsync();

        while (true)
        {
            if(DateTime.Now.Hour == 6 && DateTime.Now.Minute == 00)
            {
                Console.Clear();
                IniciarPrograma();
            }
            else
            {
                Console.WriteLine("Aguardando o horário correto para iniciar o programa...");
                System.Threading.Thread.Sleep(60000);
            }
        }
    }

    static void IniciarPrograma()
    {
        Console.WriteLine("Programa iniciado. Aguardando execução...");

        CriarNotificacoes();
        VerificarEEnviarEmails();
    }

    static void CriarNotificacoes()
    {
        DatabaseHelper.ExecuteStoreProcedure("CriaNotificacao");
    }

    static void VerificarEEnviarEmails()
    {
        // Query para buscar usuários que devem receber o e-mail
        string queryUsuarios = string.Format(@"
                                            DECLARE @DiasRetorno int = (SELECT TOP 1 ISNULL(DiasNotificacaoRetorno, 0) FROM Setup)
                                            
                                            SELECT
                                                N.Id Notificacao_Id,
	                                            CONVERT(DATE, CC.DataRetorno, 103) DataRetorno,
	                                            M.Identificacao  + ' ' + M.Concentracao + ' ' + U.Identificacao Medicamento,
	                                            P.Nome Nome,
	                                            P.Email
                                            FROM Notificacoes N
                                            JOIN CartaoControle CC on CC.Id = N.CartaoControle_Id
                                            JOIN Medicamentos M on M.Id = CC.Medicamento_Id
                                            JOIN Pacientes P on P.Id = CC.Paciente_Id
                                            JOIN Unidades U on U.Id = M.Unidade_Id
                                            WHERE DATEDIFF(DAY, GETDATE(), CC.DataRetorno) BETWEEN 0 AND @DiasRetorno
                                            AND N.Tipo = 'NotificacaoRetorno'
                                            AND CONVERT(DATE, N.Data, 103) = CONVERT(DATE, GETDATE(), 103)
                                            AND ISNULL(N.Enviado, 0) = 0
                                            AND ISNULL(P.Deletado, 0) = 0");

        string queryEmailTemplate = "SELECT [Titulo], [Corpo] FROM Emails WHERE Identificacao = 'NotificacaoRetorno' AND Ativo = 1";

        try
        {
            DataTable emailTemplateTable = DatabaseHelper.GetDataTable(queryEmailTemplate);
            DataTable usuariosTable = DatabaseHelper.GetDataTable(queryUsuarios);

            string titulo = string.Empty;
            string corpo = string.Empty;

            if (emailTemplateTable.Rows.Count > 0)
            {
                titulo = emailTemplateTable.Rows[0]["Titulo"].ToString();
                corpo = emailTemplateTable.Rows[0]["Corpo"].ToString();

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

                    // Query para marcar a Notificacao como "Enviada"
                    string queryUpdateNotificacao = string.Format(@"UPDATE Notificacoes SET Enviado = 1 WHERE Id = '{0}'", row["Notificacao_Id"].ToString());
                    DatabaseHelper.Update(queryUpdateNotificacao);
                }

                Console.WriteLine("E-mails enviados com sucesso!");
            }
            else
                Console.Error.WriteLine("ERRO! Os E-mails não foram enviados, pois esse tipo de Notificação não possui um template ativo!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    // Configuração do servidor HTTP
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.Run(async context =>
                    {
                        // Resposta simples para manter o servidor ativo
                        await context.Response.WriteAsync("Servidor em execução...");
                    });
                })
                .UseUrls("http://0.0.0.0:10000"); // Escuta na porta 10000
            });
}
