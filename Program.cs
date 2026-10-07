using System;
using System.Data;
using System.Data.Common;

namespace ConsoleApp8
{
    internal class Program
    {
        // Активный провайдер: new SqliteProvider() или new NpgsqlProvider()
        private static readonly DbProvider Provider = new NpgsqlProvider();

        private static void Main(string[] args)
        {
            // 1. Фабрика провайдера
            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider.Name);

            Console.WriteLine("Провайдер:   " + Provider.Name);
            Console.WriteLine("Фабрика:     " + factory.GetType().FullName);

            // Мои данные
            const string lastName = "Малыгин";
            const string firstName = "Александр";

            try
            {
                // 2. Подключение к базе
                using (DbConnection connection = CreateConnection(factory))
                {
                    // 3. Сколько моих строк уже в таблице
                    int myRows = CountPersons(connection, factory, lastName, firstName);

                    // Если их нет — добавляю, если есть — пропускаю
                    if (myRows == 0)
                    {
                        Console.WriteLine("Моих записей нет — добавляю.");
                        InsertPerson(connection, factory, lastName, firstName);
                    }
                    else
                    {
                        Console.WriteLine("Мои записи уже есть (найдено: " + myRows + ") — добавлять не нужно.");
                    }

                    // 4. Читаю свои строки через DbDataReader
                    ShowPersons(connection, factory, lastName, firstName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nОшибка при работе с базой: " + ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Готово. Нажмите Enter, чтобы закрыть.");
            Console.ReadLine();
        }

        // Создание подключения
        private static DbConnection CreateConnection(DbProviderFactory factory)
        {
            DbConnection connection = factory.CreateConnection();
            if (connection == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbConnection.");
            }

            connection.ConnectionString = Provider.GetConnectionString();
            connection.Open();
            return connection;
        }

        // Создание команды
        private static DbCommand CreateCommand(DbConnection connection, DbProviderFactory factory, string sql)
        {
            DbCommand command = factory.CreateCommand();
            if (command == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbCommand.");
            }

            command.Connection = connection;
            command.CommandText = sql;
            return command;
        }

        // Создание параметра
        private static void AddParameter(DbProviderFactory factory, DbCommand command, string name, object value)
        {
            DbParameter parameter = factory.CreateParameter();
            if (parameter == null)
            {
                throw new Exception("Провайдер " + Provider.Name + " не умеет создавать DbParameter.");
            }

            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        // Сколько моих строк в таблице
        private static int CountPersons(DbConnection connection, DbProviderFactory factory, string lastName, string firstName)
        {
            using (DbCommand command = CreateCommand(connection, factory,
                "SELECT COUNT(*) FROM test01 WHERE last_name = @lastName AND first_name = @firstName"))
            {
                AddParameter(factory, command, "@lastName", lastName);
                AddParameter(factory, command, "@firstName", firstName);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // Добавление своей строки
        private static void InsertPerson(DbConnection connection, DbProviderFactory factory, string lastName, string firstName)
        {
            using (DbCommand command = CreateCommand(connection, factory,
                "INSERT INTO test01 (last_name, first_name) VALUES (@lastName, @firstName)"))
            {
                AddParameter(factory, command, "@lastName", lastName);
                AddParameter(factory, command, "@firstName", firstName);

                command.ExecuteNonQuery();
            }
        }

        // Чтение своих строк
        private static void ShowPersons(DbConnection connection, DbProviderFactory factory, string lastName, string firstName)
        {
            using (DbCommand command = CreateCommand(connection, factory,
                "SELECT id, last_name, first_name FROM test01 " +
                "WHERE last_name = @lastName AND first_name = @firstName ORDER BY id"))
            {
                AddParameter(factory, command, "@lastName", lastName);
                AddParameter(factory, command, "@firstName", firstName);

                using (DbDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine(
                        $"\n{"ID: ",-4} | " +
                        $"{"last_name: ",-15} | " +
                        $"{"first_name: ",-15} | ");

                    int count = 0;
                    while (reader.Read())
                    {
                        Console.WriteLine(
                            $"{reader.GetInt32(0),-4} | " +
                            $"{reader.GetString(1),-15} | " +
                            $"{reader.GetString(2),-15} | ");
                        count++;
                    }

                    if (count == 0)
                    {
                        Console.WriteLine("(записей не найдено)");
                    }
                }
            }
        }
    }

    // Общий предок провайдеров
    internal abstract class DbProvider
    {
        public abstract string Name { get; }

        public abstract string GetConnectionString();
    }
}
