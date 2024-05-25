using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Library_Project_1
{
    class DBAccess
    {
        private static SqlConnection connection = new SqlConnection();
        private static SqlCommand command = new SqlCommand();
        private static SqlDataReader DbReader;
        private static SqlDataAdapter adapter = new SqlDataAdapter();
        public SqlTransaction DbTran;

        private static string strConnString = "Data Source=(local);Initial Catalog=\"Library project\";Integrated Security=True";

        // Open connection to the database
        public void createConn()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.ConnectionString = strConnString;
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Close connection to the database
        public void closeConn()
        {
            connection.Close();
        }

        // Execute data adapter for updating the database
        public int executeDataAdapter(DataTable tblName, string strSelectSql)
        {
            try
            {
                if (connection.State == 0)
                {
                    createConn();
                }

                adapter.SelectCommand.CommandText = strSelectSql;
                adapter.SelectCommand.CommandType = CommandType.Text;
                SqlCommandBuilder DbCommandBuilder = new SqlCommandBuilder(adapter);

                string insert = DbCommandBuilder.GetInsertCommand().CommandText.ToString();
                string update = DbCommandBuilder.GetUpdateCommand().CommandText.ToString();
                string delete = DbCommandBuilder.GetDeleteCommand().CommandText.ToString();

                return adapter.Update(tblName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Read data using data adapter
        public void readDatathroughAdapter(string query, DataTable tblName)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    createConn();
                }

                command.Connection = connection;
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                adapter = new SqlDataAdapter(command);
                adapter.Fill(tblName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Read data using data reader
        public SqlDataReader readDatathroughReader(string query)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    createConn();
                }

                command.Connection = connection;
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                DbReader = command.ExecuteReader();
                return DbReader;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Execute a query
        public int executeQuery(SqlCommand dbCommand)
        {
            try
            {
                if (connection.State == 0)
                {
                    createConn();
                }

                dbCommand.Connection = connection;
                dbCommand.CommandType = CommandType.Text;

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Check if an email exists in the database
        public bool IsEmailExists(string email)
        {
            try
            {
                if (connection.State == 0)
                {
                    createConn();
                }

                command.Connection = connection;
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                command.CommandType = CommandType.Text;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Email", email);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Execute a scalar query
        public object ExecuteScalar(SqlCommand dbCommand)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    createConn();
                }

                dbCommand.Connection = connection;
                dbCommand.CommandType = CommandType.Text;

                return dbCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Execute a scalar query
        public object executeScalar(string query)
        {
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                object result = command.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

    }
}
