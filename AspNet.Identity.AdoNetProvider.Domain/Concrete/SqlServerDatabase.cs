using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace AspNet.Identity.AdoNetProvider.Domain.Concrete
{
    public class SqlServerDatabase : IDisposable
    {
        private SqlConnection _connection;

        public SqlServerDatabase(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public void Dispose()
        {
            if (_connection == null)
            {
                return;
            }

            _connection.Dispose();
            _connection = null;
        }

        #region ADO.NET wrapper methods

        /// <summary>
        ///     Executes a T-SQL query in the database and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>The number of rows affected by the T-SQL query.</returns>
        public int ExecuteNonQuery(string commandText, Dictionary<string, object> sqlParameters)
        {
            int numberOfRowsAffected;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                var command = CreateCommand(commandText, sqlParameters);
                numberOfRowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            return numberOfRowsAffected;
        }

        /// <summary>
        ///     Executes a T-SQL query and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>The first column of the first row in the result set returned by the query.</returns>
        public object ExecuteScalar(string commandText, Dictionary<string, object> sqlParameters)
        {
            object result;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                var command = CreateCommand(commandText, sqlParameters);
                result = command.ExecuteScalar();
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            return result;
        }

        /// <summary>
        ///     Executes a T-SQL query and returns the results as a SqlDataReader object.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>A SqlDataReader object containing the results of the T-SQL query.</returns>
        public List<Dictionary<string, string>> ExecuteReader(string commandText,
            Dictionary<string, object> sqlParameters)
        {
            List<Dictionary<string, string>> rows;
            SqlDataReader reader;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                rows = new List<Dictionary<string, string>>();
                var command = CreateCommand(commandText, sqlParameters);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new Dictionary<string, string>();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                        row.Add(columnName, columnValue);
                    }

                    rows.Add(row);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            reader.Close();
            return rows;
        }

        #endregion

        #region Asynchronous versions of ADO.NET wrapper methods

        /// <summary>
        ///     Asynchronously executes a T-SQL query in the database and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>The number of rows affected by the T-SQL query.</returns>
        public async Task<int> ExecuteNonQueryAsync(string commandText, Dictionary<string, object> sqlParameters)
        {
            int numberOfRowsAffected;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                var command = CreateCommand(commandText, sqlParameters);
                numberOfRowsAffected = await command.ExecuteNonQueryAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            return numberOfRowsAffected;
        }

        /// <summary>
        ///     Asynchronously executes a T-SQL query and returns the first column of the first row in the result set returned by
        ///     the query.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>The first column of the first row in the result set returned by the query.</returns>
        public async Task<object> ExecuteScalarAsync(string commandText, Dictionary<string, object> sqlParameters)
        {
            object result;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                var command = CreateCommand(commandText, sqlParameters);
                result = await command.ExecuteScalarAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            return result;
        }

        /// <summary>
        ///     Asynchronously executes a T-SQL query and returns the results as a SqlDataReader object.
        /// </summary>
        /// <param name="commandText">The T-SQL statement to execute.</param>
        /// <param name="sqlParameters">The parameters of the T-SQL query.</param>
        /// <returns>A SqlDataReader object containing the results of the T-SQL query.</returns>
        public async Task<List<Dictionary<string, string>>> ExecuteReaderAsync(string commandText, Dictionary<string, object> sqlParameters)
        {
            List<Dictionary<string, string>> rows;
            SqlDataReader reader;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("The T-SQL command cannot be null or empty.");
            }

            try
            {
                EnsureOpenConnection();
                rows = new List<Dictionary<string, string>>();
                var command = CreateCommand(commandText, sqlParameters);
                reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    var row = new Dictionary<string, string>();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                        row.Add(columnName, columnValue);
                    }

                    rows.Add(row);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("An error occured: " + exception.Message);
            }
            finally
            {
                EnsureClosedConnection();
            }

            reader.Close();
            return rows;
        }

        #endregion

        #region Private Methods

        private SqlCommand CreateCommand(string commandText, Dictionary<string, object> sqlParameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = commandText;
            AddCommandParameters(command, sqlParameters);

            return command;
        }

        private static void AddCommandParameters(SqlCommand command, Dictionary<string, object> sqlParameters)
        {
            if (sqlParameters.Count == 0)
            {
                return;
            }

            foreach (var sqlParameter in sqlParameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = sqlParameter.Key;
                parameter.Value = sqlParameter.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        private void EnsureOpenConnection()
        {
            var numberOfTries = 3;

            if (_connection.State == ConnectionState.Open)
            {
                return;
            }

            while (numberOfTries >= 0 && _connection.State != ConnectionState.Open)
            {
                _connection.Open();
                numberOfTries--;
                Thread.Sleep(30);
            }
        }

        private void EnsureClosedConnection()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        #endregion
    }
}