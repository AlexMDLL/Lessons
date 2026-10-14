using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace ConsoleApp7
{
    internal class Practice
    {
        private DataTable dataTable;
        private bool isDataLoaded;

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

        public async Task ExecuteAsync()
        {
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("    Меню   ");
                Console.WriteLine("1. Загрузить данные из БД (локальная копия)");
                Console.WriteLine("2. Изменить имя студента по ID");
                Console.WriteLine("3. Добавить нового студента");
                Console.WriteLine("4. Удалить студента по ID");
                Console.WriteLine("5. Показать журнал изменений (Practice 2)");
                Console.WriteLine("6. Показать количество строк по состояниям");
                Console.WriteLine("7. Отменить все изменения (RejectChanges)");
                Console.WriteLine("8. Принять все изменения (AcceptChanges)");
                Console.WriteLine("9. Показать текущую таблицу");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                switch (input)
                {
                    case "1":
                        await this.LoadDataAsync();
                        break;
                    case "2":
                        this.ModifyStudentById();
                        break;
                    case "3":
                        this.AddStudent();
                        break;
                    case "4":
                        this.DeleteStudentById();
                        break;
                    case "5":
                        this.ShowChangeJournal();
                        break;
                    case "6":
                        this.ShowStateCounts();
                        break;
                    case "7":
                        this.RejectAllChanges();
                        break;
                    case "8":
                        this.AcceptAllChanges();
                        break;
                    case "9":
                        this.PrintDataTable();
                        break;
                    case "0":
                        isRunning = false;
                        Console.WriteLine("Выход из программы.");
                        break;
                    default:
                        Console.WriteLine("Неверный ввод. Попробуйте снова.");
                        break;
                }

                if (isRunning)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private async Task LoadDataAsync()
        {
            if (this.isDataLoaded)
            {
                Console.WriteLine("Данные уже загружены.");
                return;
            }

            string connectionString = this.GetConnectionString();
            DbProviderFactory factory = NpgsqlFactory.Instance;

            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            DbCommand command = factory.CreateCommand();
            command.Connection = connection;
            command.CommandText = "SELECT * FROM students_lesson13";

            DbDataAdapter dataAdapter = factory.CreateDataAdapter();
            dataAdapter.SelectCommand = command;

            this.dataTable = new DataTable();

            await connection.OpenAsync();
            dataAdapter.Fill(this.dataTable);
            connection.Close();
            connection.Dispose();
            command.Dispose();
            dataAdapter.Dispose();

            this.isDataLoaded = true;
            Console.WriteLine("Данные успешно загружены в локальную копию.");
            this.PrintDataTable();
        }

        private void ModifyStudentById()
        {
            this.EnsureDataLoaded();
            Console.Write("Введите ID студента для изменения: ");
            string idInput = Console.ReadLine();

            if (!int.TryParse(idInput, out int studentId))
            {
                Console.WriteLine("Некорректный ID.");
                return;
            }

            DataRow[] foundRows = this.dataTable.Select($"id = {studentId}");
            if (foundRows.Length == 0)
            {
                Console.WriteLine("Студент с таким ID не найден.");
                return;
            }

            DataRow targetRow = foundRows[0];
            Console.WriteLine($"Текущее имя: {targetRow["first_name"]} {targetRow["last_name"]}");
            Console.Write("Введите новое имя: ");
            string newName = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newName))
            {
                targetRow["first_name"] = newName;
                Console.WriteLine("Имя успешно изменено. Состояние строки теперь: Modified.");
            }
        }

        private void AddStudent()
        {
            this.EnsureDataLoaded();
            DataRow newRow = this.dataTable.NewRow();

            Console.Write("Введите псевдоним (nickname): ");
            newRow["nickname"] = Console.ReadLine();

            Console.Write("Введите имя (first_name): ");
            newRow["first_name"] = Console.ReadLine();

            Console.Write("Введите фамилию (last_name): ");
            newRow["last_name"] = Console.ReadLine();

            Console.Write("Введите группу (group): ");
            newRow["group"] = Console.ReadLine();

            this.dataTable.Rows.Add(newRow);
            Console.WriteLine("Студент успешно добавлен. Состояние строки: Added.");
        }

        private void DeleteStudentById()
        {
            this.EnsureDataLoaded();
            Console.Write("Введите ID студента для удаления: ");
            string idInput = Console.ReadLine();

            if (!int.TryParse(idInput, out int studentId))
            {
                Console.WriteLine("Некорректный ID.");
                return;
            }

            DataRow[] foundRows = this.dataTable.Select($"id = {studentId}");
            if (foundRows.Length == 0)
            {
                Console.WriteLine("Студент с таким ID не найден.");
                return;
            }

            foundRows[0].Delete();
            Console.WriteLine("Студент помечен как удалённый. Состояние строки: Deleted.");
        }

        private void ShowChangeJournal()
        {
            this.EnsureDataLoaded();
            Console.WriteLine("    Журнал изменений    ");

            int addedCount = 0;
            int modifiedCount = 0;
            int deletedCount = 0;

            foreach (DataRow row in this.dataTable.Rows)
            {
                if (row.RowState == DataRowState.Modified)
                {
                    modifiedCount++;
                    int id = Convert.ToInt32(row["id", DataRowVersion.Current]);
                    string originalName = row["first_name", DataRowVersion.Original].ToString();
                    string currentName = row["first_name", DataRowVersion.Current].ToString();
                    Console.WriteLine($"[MODIFIED] ID: {id}, {originalName} → {currentName}");
                }
                else if (row.RowState == DataRowState.Added)
                {
                    addedCount++;
                    int id = Convert.ToInt32(row["id", DataRowVersion.Current]);
                    string firstName = row["first_name", DataRowVersion.Current].ToString();
                    string lastName = row["last_name", DataRowVersion.Current].ToString();
                    string group = row["group", DataRowVersion.Current].ToString();
                    Console.WriteLine($"[ADDED] ID: {id}, {firstName} {lastName} ({"/group/"})");
                }
                else if (row.RowState == DataRowState.Deleted)
                {
                    deletedCount++;
                    // Для удалённых строк версия Current недоступна, читаем Original
                    int id = Convert.ToInt32(row["id", DataRowVersion.Original]);
                    string firstName = row["first_name", DataRowVersion.Original].ToString();
                    string lastName = row["last_name", DataRowVersion.Original].ToString();
                    string group = row["group", DataRowVersion.Original].ToString();
                    Console.WriteLine($"[DELETED] ID: {id}, {firstName} {lastName} ({group})");
                }
            }

            Console.WriteLine("\n    Статистика состояний    ");
            Console.WriteLine($"Added: {addedCount}");
            Console.WriteLine($"Modified: {modifiedCount}");
            Console.WriteLine($"Deleted: {deletedCount}");
        }

        private void ShowStateCounts()
        {
            this.EnsureDataLoaded();
            int addedCount = 0;
            int modifiedCount = 0;
            int deletedCount = 0;
            int unchangedCount = 0;

            foreach (DataRow row in this.dataTable.Rows)
            {
                switch (row.RowState)
                {
                    case DataRowState.Added:
                        addedCount++;
                        break;
                    case DataRowState.Modified:
                        modifiedCount++;
                        break;
                    case DataRowState.Deleted:
                        deletedCount++;
                        break;
                    case DataRowState.Unchanged:
                        unchangedCount++;
                        break;
                }
            }

            Console.WriteLine("    Количество строк по состояниям    ");
            Console.WriteLine($"Unchanged: {unchangedCount}");
            Console.WriteLine($"Added: {addedCount}");
            Console.WriteLine($"Modified: {modifiedCount}");
            Console.WriteLine($"Deleted: {deletedCount}");
        }

        private void RejectAllChanges()
        {
            this.EnsureDataLoaded();
            this.dataTable.RejectChanges();
            Console.WriteLine("Все изменения отменены. Таблица возвращена к последнему принятому состоянию.");
            this.ShowStateCounts();
        }

        private void AcceptAllChanges()
        {
            this.EnsureDataLoaded();
            this.dataTable.AcceptChanges();
            Console.WriteLine("Все изменения приняты. Состояния строк обновлены на Unchanged.");
            this.ShowStateCounts();
        }

        private void EnsureDataLoaded()
        {
            if (!this.isDataLoaded || this.dataTable == null)
            {
                throw new Exception("Сначала загрузите данные из БД (выберите пункт 1).");
            }
        }

        private void PrintDataTable()
        {
            this.EnsureDataLoaded();
            Console.WriteLine(
                $"{"ID",-10} |" +
                $" {"Никнейм",-20} |" +
                $" {"Имя",-20} |" +
                $" {"Фамилия",-20} |" +
                $" {"Группа",-15}"
            );
            Console.WriteLine(new string('-', 95));
            foreach (DataRow row in this.dataTable.Rows)
            {
                Console.WriteLine(
                    $"#{row["id"],-9} |" +
                    $" {row["nickname"],-20} |" +
                    $" {row["first_name"],-20} |" +
                    $" {row["last_name"],-20} |" +
                    $" {row["group"],-15}"
                );
            }
        }
    }
}