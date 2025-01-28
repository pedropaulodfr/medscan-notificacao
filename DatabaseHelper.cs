using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace medscanner_notificacao
{
    internal class DatabaseHelper
    {
        public static string StringConnection()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
        public static DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(StringConnection()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao executar a query: {ex.Message}");
                throw;
            }

            return dataTable;
        }

        public static void ExecuteStoreProcedure(string procedure)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(StringConnection()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(procedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.ExecuteNonQuery();

                        Console.WriteLine("Procedure " + procedure + " executada com sucesso.");
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Erro ao executar a procedure " + procedure + ": " + ex.Message);
            }
        }

        public static void Update(string query)
        {
            try
            {
                using(SqlConnection connection = new SqlConnection(StringConnection()))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
