using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace WpfApp26.Data
{
    // Работа с базой через фабрику провайдера (DbProviderFactory)
    internal static class Db
    {
        // Активный провайдер
        public static readonly DbProvider Provider = new NpgsqlProvider();

        // 1. Фабрика провайдера
        public static DbProviderFactory GetFactory()
        {
            try
            {
                return DbProviderFactories.GetFactory(Provider.Name);
            }
            catch (Exception ex)
            {
                throw new Exception("Провайдер " + Provider.Name
                    + " не зарегистрирован в App.config (раздел system.data / DbProviderFactories). " + ex.Message);
            }
        }

        // 2. Подключение
        public static async Task<DbConnection> OpenConnectionAsync()
        {
            DbConnection connection = GetFactory().CreateConnection();
            if (connection == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbConnection.");
            }

            connection.ConnectionString = Provider.GetConnectionString();
            await connection.OpenAsync();
            return connection;
        }

        // 3. Команда
        public static DbCommand CreateCommand(DbConnection connection, string sql)
        {
            DbCommand command = GetFactory().CreateCommand();
            if (command == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbCommand.");
            }

            command.Connection = connection;
            command.CommandText = sql;
            return command;
        }

        // 4. Параметр
        public static void AddParameter(DbCommand command, string name, object value)
        {
            DbParameter parameter = GetFactory().CreateParameter();
            if (parameter == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbParameter.");
            }

            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        // Короткая расшифровка ошибки для строки состояния
        public static string ErrorCode(Exception ex)
        {
            if (ex is System.IO.FileNotFoundException) return " (нет файла db.local.txt)";
            if (ex is System.Net.Sockets.SocketException) return " (нет сети)";
            if (ex is TimeoutException) return " (таймаут)";

            return Provider.CodeFor(ex);
        }

        // Подсказка, что делать с ошибкой
        public static string Hint(Exception ex)
        {
            if (ex is System.IO.FileNotFoundException)
                return "Создайте db.local.txt рядом с WpfApp26.csproj по образцу db.local.example.txt.";

            if (ex is System.Net.Sockets.SocketException)
                return "Сервер не отвечает: проверьте Host и Port в db.local.txt.";

            if (ex is TimeoutException)
                return "Превышено время ожидания ответа сервера.";

            string hint = Provider.HintFor(ex);
            return hint.Length > 0 ? hint : "Проверьте строку подключения в db.local.txt.";
        }
    }
}
