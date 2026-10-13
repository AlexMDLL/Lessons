using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Npgsql;

namespace ConsoleApp11
{
    public class DatabaseManager
    {
        public string GetConnectionString()
        {
            const string fileName = "db.local.txt";

            for (DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); directoryInfo != null; directoryInfo = directoryInfo.Parent)
            {
                string path = Path.Combine(directoryInfo.FullName, fileName);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }

            throw new Exception("Не найден файл " + fileName + " со строкой подключения к БД.");
        }

        public string GetPersonalConnectionString()
        {
            const string fileName = "db.local.ME.txt";

            for (DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); directoryInfo != null; directoryInfo = directoryInfo.Parent)
            {
                    string path = Path.Combine(directoryInfo.FullName, fileName);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }

            throw new Exception("Не найден файл " + fileName + " со строкой подключения к БД.");
        }

        public void InsertStudentFromConsole(string connectionString)
        {
            Console.WriteLine("Введите псевдоним (nickname):");
            string nickName = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Введите имя (first_name):");
            string firstName = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Введите фамилию (last_name):");
            string lastName = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Введите имя группы (group):");
            string groupName = Console.ReadLine() ?? string.Empty;

            NpgsqlConnection checkConnection = new NpgsqlConnection(connectionString);
            NpgsqlCommand checkCommand = new NpgsqlCommand(
                $"SELECT COUNT(*) FROM students_lesson13 WHERE nickname = '{nickName}'",
                checkConnection
            );

            try
            {
                checkConnection.Open();
                int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (existingCount > 0)
                {
                    Console.WriteLine("Ошибка: Студент с таким псевдонимом уже существует в базе данных.");
                    return;
                }
            }
            finally
            {
                checkCommand.Dispose();
                checkConnection.Dispose();
            }

            NpgsqlConnection insertConnection = new NpgsqlConnection(connectionString);
            NpgsqlCommand insertCommand = new NpgsqlCommand(
                $"INSERT INTO students_lesson13 (nickname, first_name, last_name, \"group\") VALUES ('{nickName}', '{firstName}', '{lastName}', '{groupName}')",
                insertConnection
            );

            try
            {
                insertConnection.Open();
                int affectedRows = insertCommand.ExecuteNonQuery();
                Console.WriteLine($"Количество затронутых строк при вставке: {affectedRows}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка базы данных: Данные уже существуют или нарушены ограничения целостности. {exception.Message}");
            }
            finally
            {
                insertCommand.Dispose();
                insertConnection.Dispose();
            }
        }

        public void LoadStudentsAndPrint(string connectionString)
        {
            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory("Npgsql");
            }
            catch (ArgumentException)
            {
                factory = NpgsqlFactory.Instance;
            }

            DataTable dataTable = new DataTable();
            DbConnection connection = factory.CreateConnection() ?? throw new InvalidOperationException("Не удалось создать соединение с базой данных.");
            DbCommand command = connection.CreateCommand();
            DbDataAdapter dataAdapter = factory.CreateDataAdapter() ?? throw new InvalidOperationException("Не удалось создать адаптер данных.");

            try
            {
                connection.ConnectionString = connectionString;
                command.CommandText = "SELECT id, nickname, first_name, last_name, \"group\" FROM students_lesson13 ORDER BY id;";
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataTable);
            }
            finally
            {
                dataAdapter.Dispose();
                command.Dispose();
                connection.Dispose();
            }

            Console.WriteLine(
                $"{"ID",-10} |" +
                $" {"Псевдоним",-20} |" +
                $" {"Имя",-20} |" +
                $" {"Фамилия",-20} |" +
                $" {"Группа",-15}"
            );
            Console.WriteLine(new string('-', 95));

            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine(
                    $"#{row["id"],-9} |" +
                    $" {row["nickname"],-20} |" +
                    $" {row["first_name"],-20} |" +
                    $" {row["last_name"],-20} |" +
                    $" {row["group"],-15}"
                );
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine($"Всего записей: {dataTable.Rows.Count}");
        }

        public void CopyDataToPersonalDatabase()
        {
            string sourceConnectionString = this.GetConnectionString();
            string destinationConnectionString = this.GetPersonalConnectionString();

            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory("Npgsql");
            }
            catch (ArgumentException)
            {
                factory = NpgsqlFactory.Instance;
            }

            DataTable dataTable = new DataTable();
            DbConnection sourceConnection = factory.CreateConnection() ?? throw new InvalidOperationException("Не удалось создать соединение с исходной базой данных.");
            DbCommand selectCommand = sourceConnection.CreateCommand();
            DbDataAdapter dataAdapter = factory.CreateDataAdapter() ?? throw new InvalidOperationException("Не удалось создать адаптер данных.");

            try
            {
                sourceConnection.ConnectionString = sourceConnectionString;
                selectCommand.CommandText = "SELECT id, nickname, first_name, last_name, \"group\" FROM students_lesson13 ORDER BY id;";
                dataAdapter.SelectCommand = selectCommand;
                dataAdapter.Fill(dataTable);
            }
            finally
            {
                dataAdapter.Dispose();
                selectCommand.Dispose();
                sourceConnection.Dispose();
            }

            Console.WriteLine($"Из общей базы загружено записей: {dataTable.Rows.Count}");

            if (dataTable.Rows.Count == 0)
            {
                Console.WriteLine("Нечего переносить.");
                return;
            }

            NpgsqlConnection destinationConnection = new NpgsqlConnection(destinationConnectionString);

            try
            {
                destinationConnection.Open();

                NpgsqlCommand createTableCommand = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS students_lesson13 (" +
                    "id INTEGER PRIMARY KEY, " +
                    "nickname VARCHAR(100), " +
                    "first_name VARCHAR(100), " +
                    "last_name VARCHAR(100), " +
                    "\"group\" VARCHAR(100)" +
                    ");",
                    destinationConnection
                );
                createTableCommand.ExecuteNonQuery();
                createTableCommand.Dispose();

                int copiedCount = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    string nickName = row["nickname"].ToString() ?? string.Empty;
                    string firstName = row["first_name"].ToString() ?? string.Empty;
                    string lastName = row["last_name"].ToString() ?? string.Empty;
                    string groupName = row["group"].ToString() ?? string.Empty;
                    int id = Convert.ToInt32(row["id"]);

                    nickName = nickName.Replace("'", "''");
                    firstName = firstName.Replace("'", "''");
                    lastName = lastName.Replace("'", "''");
                    groupName = groupName.Replace("'", "''");

                    NpgsqlCommand insertCommand = new NpgsqlCommand(
                        $"INSERT INTO students_lesson13 (id, nickname, first_name, last_name, \"group\") VALUES ({id}, '{nickName}', '{firstName}', '{lastName}', '{groupName}') ON CONFLICT (id) DO NOTHING;",
                        destinationConnection
                    );

                    insertCommand.ExecuteNonQuery();
                    insertCommand.Dispose();
                    copiedCount++;
                }

                Console.WriteLine($"Успешно перенесено записей в личную базу: {copiedCount}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка при переносе данных: {exception.Message}");
            }
            finally
            {
                destinationConnection.Dispose();
            }
        }
    }
}