using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

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
                Console.WriteLine("2. Изменить данные студента по ID");
                Console.WriteLine("3. Добавить нового студента");
                Console.WriteLine("4. Удалить студента по ID");
                Console.WriteLine("5. Показать журнал изменений");
                Console.WriteLine("6. Показать количество строк по состояниям");
                Console.WriteLine("7. Отменить все изменения (RejectChanges)");
                Console.WriteLine("8. Принять все изменения в памяти (AcceptChanges)");
                Console.WriteLine("9. Фильтрация и сортировка (Practice 4)");
                Console.WriteLine("10. Сохранить изменения в БД (Practice 3)");
                Console.WriteLine("11. Показать текущую таблицу");
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
                        this.FilterAndSortStudents();
                        break;
                    case "10":
                        await this.SaveChangesAsync();
                        break;
                    case "11":
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
            command.CommandText = "SELECT * FROM students_lesson14";

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
            Console.WriteLine($"Текущие данные: {targetRow["first_name"]} {targetRow["last_name"]}, Возраст: {targetRow["age"]}");

            Console.Write("Введите новое имя (оставьте пустым, чтобы не менять): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                targetRow["first_name"] = newName;
            }

            Console.Write("Введите новый возраст (оставьте пустым, чтобы не менять): ");
            string newAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newAgeInput) && int.TryParse(newAgeInput, out int newAge))
            {
                targetRow["age"] = newAge;
            }

            Console.WriteLine("Данные успешно изменены. Состояние строки: Modified.");
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

          //  Console.Write("Введите группу (group): ");
           // newRow["group"] = Console.ReadLine();

            Console.Write("Введите возраст (age): ");
            string ageInput = Console.ReadLine();
            newRow["age"] = int.TryParse(ageInput, out int age) ? age : 0;

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

        private void FilterAndSortStudents()
        {
            this.EnsureDataLoaded();

            Console.WriteLine("ВНИМАНИЕ\nПример фильтра: age > 18\n Нажмите любую клавишу ");
            Console.ReadLine();
            Console.Write("Введите условие фильтрации (RowFilter) или нажмите Enter для сброса: ");
            string filter = Console.ReadLine();

            Console.WriteLine("ВНИМАНИЕ\nПример сортировки: first_name ASC ИЛИ age DESC\n Нажмите любую клавишу ");
            Console.ReadLine();
            Console.Write("Введите условие сортировки (Sort) или нажмите Enter для сброса: ");
            string sort = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filter))
            {
                this.dataTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                this.dataTable.DefaultView.RowFilter = filter;
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                this.dataTable.DefaultView.Sort = string.Empty;
            }
            else
            {
                this.dataTable.DefaultView.Sort = sort;
            }

            Console.WriteLine("\n    Отфильтрованные и отсортированные данные (DefaultView)    ");
            this.PrintDataView(this.dataTable.DefaultView);
        }

        //private async Task SaveChangesAsync()
        //{
        //    this.EnsureDataLoaded();

        //    string connectionString = this.GetConnectionString();
        //    DbProviderFactory factory = NpgsqlFactory.Instance;
        //    DbConnection connection = factory.CreateConnection();
        //    connection.ConnectionString = connectionString;
        //    DbCommand selectCommand = factory.CreateCommand();
        //    selectCommand.Connection = connection;
        //    selectCommand.CommandText = "SELECT id, nickname, first_name, last_name,  age FROM students_lesson14";//  \"group\",
        //    DbDataAdapter dataAdapter = factory.CreateDataAdapter();
        //    dataAdapter.SelectCommand = selectCommand;

        //    //  1. INSERT 
        //    DbCommand insertCommand = factory.CreateCommand();
        //    insertCommand.Connection = connection;
        //    insertCommand.CommandText = "INSERT INTO students_lesson14 (nickname, first_name, last_name,  age) VALUES (@nickname, @first_name, @last_name,  @age)";// \"group\", @group,
        //    insertCommand.Parameters.Add(new NpgsqlParameter("@nickname", NpgsqlDbType.Varchar));
        //    insertCommand.Parameters.Add(new NpgsqlParameter("@first_name", NpgsqlDbType.Varchar));
        //    insertCommand.Parameters.Add(new NpgsqlParameter("@last_name", NpgsqlDbType.Varchar));
        //    //insertCommand.Parameters.Add(new NpgsqlParameter("@group", NpgsqlDbType.Varchar));
        //    insertCommand.Parameters.Add(new NpgsqlParameter("@age", NpgsqlDbType.Integer));

        //    //  2. UPDATE 
        //    DbCommand updateCommand = factory.CreateCommand();
        //    updateCommand.Connection = connection;
        //    updateCommand.CommandText = "UPDATE students_lesson14 SET nickname=@nickname, first_name=@first_name, last_name=@last_name,  age=@age WHERE id=@id";// \"group\"=@group,
        //    updateCommand.Parameters.Add(new NpgsqlParameter("@nickname", NpgsqlDbType.Varchar));
        //    updateCommand.Parameters.Add(new NpgsqlParameter("@first_name", NpgsqlDbType.Varchar));
        //    updateCommand.Parameters.Add(new NpgsqlParameter("@last_name", NpgsqlDbType.Varchar));
        //    //updateCommand.Parameters.Add(new NpgsqlParameter("@group", NpgsqlDbType.Varchar));
        //    updateCommand.Parameters.Add(new NpgsqlParameter("@age", NpgsqlDbType.Integer));
        //    updateCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer));

        //    //  3. DELETE 
        //    DbCommand deleteCommand = factory.CreateCommand();
        //    deleteCommand.Connection = connection;
        //    deleteCommand.CommandText = "DELETE FROM students_lesson14 WHERE id=@id"; 
        //    deleteCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer));

        //    // Привязываем команды
        //    dataAdapter.InsertCommand = insertCommand;
        //    dataAdapter.UpdateCommand = updateCommand;
        //    dataAdapter.DeleteCommand = deleteCommand;

        //    try
        //    {
        //        await connection.OpenAsync();
        //        Console.WriteLine("Начало сохранения изменений в БД...");
        //        int rowsAffected = dataAdapter.Update(this.dataTable);
        //        Console.WriteLine($"Успешно сохранено строк: {rowsAffected}");
        //        this.ShowStateCounts();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при сохранении данных в БД: {ex.Message}");
        //        this.ShowStateCounts();
        //    }
        //    finally
        //    {
        //        if (connection.State == ConnectionState.Open) { connection.Close(); }
        //        connection.Dispose();
        //        selectCommand.Dispose();
        //        dataAdapter.Dispose();
        //        insertCommand.Dispose();
        //        updateCommand.Dispose();
        //        deleteCommand.Dispose();
        //    }
        //}
        //private async Task SaveChangesAsync()
        //{
        //    this.EnsureDataLoaded();

        //    string connectionString = this.GetConnectionString();
        //    DbProviderFactory factory = NpgsqlFactory.Instance;

        //    DbConnection connection = factory.CreateConnection();
        //    connection.ConnectionString = connectionString;

        //    DbCommand selectCommand = factory.CreateCommand();
        //    selectCommand.Connection = connection;
        //    selectCommand.CommandText = "SELECT id, nickname, first_name, last_name, age FROM students_lesson14";//  \"group\",

        //    DbDataAdapter dataAdapter = factory.CreateDataAdapter();
        //    dataAdapter.SelectCommand = selectCommand;

        //    // --- 1. INSERT ---
        //    DbCommand insertCommand = factory.CreateCommand();
        //    insertCommand.Connection = connection;
        //    insertCommand.CommandText = "INSERT INTO students_lesson14 (nickname, first_name, last_name,  age) VALUES (@nickname, @first_name, @last_name,  @age)";// \"group\",@group, 

        //    // Просто создаем параметры по имени. Типы и SourceColumn не указываем.
        //    // DataAdapter сам возьмет значения из DataRow, Npgsql сам определит тип.
        //    insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@nickname" });
        //    insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@first_name" });
        //    insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@last_name" });
        //    //insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@group" });
        //    insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@age" });

        //    // --- 2. UPDATE ---
        //    DbCommand updateCommand = factory.CreateCommand();
        //    updateCommand.Connection = connection;
        //    updateCommand.CommandText = "UPDATE students_lesson14 SET nickname=@nickname, first_name=@first_name, last_name=@last_name,  age=@age WHERE id=@id";//  \"group\"=@group,

        //    updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@nickname" });
        //    updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@first_name" });
        //    updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@last_name" });
        //    //updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@group" });
        //    updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@age" });
        //    updateCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@id" });

        //    // --- 3. DELETE ---
        //    DbCommand deleteCommand = factory.CreateCommand();
        //    deleteCommand.Connection = connection;
        //    deleteCommand.CommandText = "DELETE FROM students_lesson14 WHERE id=@id";

        //    deleteCommand.Parameters.Add(new NpgsqlParameter { ParameterName = "@id" });

        //    // Привязываем команды
        //    dataAdapter.InsertCommand = insertCommand;
        //    dataAdapter.UpdateCommand = updateCommand;
        //    dataAdapter.DeleteCommand = deleteCommand;

        //    try
        //    {
        //        await connection.OpenAsync();

        //        Console.WriteLine("Начало сохранения изменений в БД...");
        //        int rowsAffected = dataAdapter.Update(this.dataTable);

        //        Console.WriteLine($"Успешно сохранено строк: {rowsAffected}");
        //        this.ShowStateCounts();
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine($"Ошибка при сохранении данных в БД: {exception.Message}");
        //        this.ShowStateCounts();
        //    }
        //    finally
        //    {
        //        if (connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }

        //        connection.Dispose();
        //        selectCommand.Dispose();
        //        dataAdapter.Dispose();
        //        insertCommand.Dispose();
        //        updateCommand.Dispose();
        //        deleteCommand.Dispose();
        //    }
        //}
        private async Task SaveChangesAsync()
        {
            this.EnsureDataLoaded();

            string connectionString = this.GetConnectionString();
            DbProviderFactory factory = NpgsqlFactory.Instance;

            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            DbCommand selectCommand = factory.CreateCommand();
            selectCommand.Connection = connection;
            selectCommand.CommandText = "SELECT id, nickname, first_name, last_name,   age FROM students_lesson14";//\"group\",

            DbDataAdapter dataAdapter = factory.CreateDataAdapter();
            dataAdapter.SelectCommand = selectCommand;

            // --- 1. INSERT ---
            DbCommand insertCommand = factory.CreateCommand();
            insertCommand.Connection = connection;
            insertCommand.CommandText = "INSERT INTO students_lesson14 (nickname, first_name, last_name,   age) VALUES (@nickname, @first_name, @last_name,    @age)";//\"group\",  @group,

            // Указываем только SourceColumn. DataAdapter сам возьмет значения из DataRow.
            insertCommand.Parameters.Add(new NpgsqlParameter("@nickname", NpgsqlDbType.Varchar) { SourceColumn = "nickname" });
            insertCommand.Parameters.Add(new NpgsqlParameter("@first_name", NpgsqlDbType.Varchar) { SourceColumn = "first_name" });
            insertCommand.Parameters.Add(new NpgsqlParameter("@last_name", NpgsqlDbType.Varchar) { SourceColumn = "last_name" });
            //insertCommand.Parameters.Add(new NpgsqlParameter("@group", NpgsqlDbType.Varchar) { SourceColumn = "group" });
            insertCommand.Parameters.Add(new NpgsqlParameter("@age", NpgsqlDbType.Integer) { SourceColumn = "age" });

            // --- 2. UPDATE ---
            DbCommand updateCommand = factory.CreateCommand();
            updateCommand.Connection = connection;
            updateCommand.CommandText = "UPDATE students_lesson14 SET nickname=@nickname, first_name=@first_name, last_name=@last_name,  age=@age WHERE id=@id";//  \"group\"=@group,

            updateCommand.Parameters.Add(new NpgsqlParameter("@nickname", NpgsqlDbType.Varchar) { SourceColumn = "nickname" });
            updateCommand.Parameters.Add(new NpgsqlParameter("@first_name", NpgsqlDbType.Varchar) { SourceColumn = "first_name" });
            updateCommand.Parameters.Add(new NpgsqlParameter("@last_name", NpgsqlDbType.Varchar) { SourceColumn = "last_name" });
            //updateCommand.Parameters.Add(new NpgsqlParameter("@group", NpgsqlDbType.Varchar) { SourceColumn = "group" });
            updateCommand.Parameters.Add(new NpgsqlParameter("@age", NpgsqlDbType.Integer) { SourceColumn = "age" });
            // Для WHERE важно брать Original ID, чтобы найти строку в БД
            updateCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { SourceColumn = "id", SourceVersion = DataRowVersion.Original });

            // --- 3. DELETE ---
            DbCommand deleteCommand = factory.CreateCommand();
            deleteCommand.Connection = connection;
            deleteCommand.CommandText = "DELETE FROM students_lesson14 WHERE id=@id";

            // Для WHERE также берем Original ID
            deleteCommand.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer) { SourceColumn = "id", SourceVersion = DataRowVersion.Original });

            // Привязываем команды к адаптеру
            dataAdapter.InsertCommand = insertCommand;
            dataAdapter.UpdateCommand = updateCommand;
            dataAdapter.DeleteCommand = deleteCommand;

            try
            {
                await connection.OpenAsync();
                Console.WriteLine("Начало сохранения изменений в БД...");
                int rowsAffected = dataAdapter.Update(this.dataTable);
                Console.WriteLine($"Успешно сохранено строк: {rowsAffected}");
                this.ShowStateCounts();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка при сохранении данных в БД: {exception.Message}");
                Console.WriteLine("Неудачные строки сохранили свои состояния (RowState) в памяти.");
                this.ShowStateCounts();
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.Dispose();
                selectCommand.Dispose();
                dataAdapter.Dispose();
                insertCommand.Dispose();
                updateCommand.Dispose();
                deleteCommand.Dispose();
            }
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
                    //string group = row["group", DataRowVersion.Current].ToString();
                    Console.WriteLine($"[ADDED] ID: {id}, {firstName} {lastName} ");//({group})
                }
                else if (row.RowState == DataRowState.Deleted)
                {
                    deletedCount++;
                    int id = Convert.ToInt32(row["id", DataRowVersion.Original]);
                    string firstName = row["first_name", DataRowVersion.Original].ToString();
                    string lastName = row["last_name", DataRowVersion.Original].ToString();
                    //string group = row["group", DataRowVersion.Original].ToString();
                    Console.WriteLine($"[DELETED] ID: {id}, {firstName} {lastName} ");//({group})
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
            // Сброс фильтрации при откате изменений
            this.dataTable.DefaultView.RowFilter = string.Empty;
            this.dataTable.DefaultView.Sort = string.Empty;
            Console.WriteLine("Все изменения отменены. Таблица возвращена к последнему принятому состоянию.");
            this.ShowStateCounts();
        }

        private void AcceptAllChanges()
        {
            this.EnsureDataLoaded();
            this.dataTable.AcceptChanges();
            Console.WriteLine("Все изменения приняты в памяти. Состояния строк обновлены на Unchanged.");
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
            this.PrintDataView(this.dataTable.DefaultView);
        }

        private void PrintDataView(DataView view)
        {
            Console.WriteLine(
                $"{"ID",-5} |" +
                $" {"Никнейм",-15} |" +
                $" {"Имя",-15} |" +
                $" {"Фамилия",-15} |" +
               // $" {"Группа",-15} |" +
                $" {"Возраст",-5}"
            );
            Console.WriteLine(new string('-', 80));

            foreach (DataRowView rowView in view)
            {
                Console.WriteLine(
                    $"→{rowView["id"],-4} |" +
                    $" {rowView["nickname"],-15} |" +
                    $" {rowView["first_name"],-15} |" +
                    $" {rowView["last_name"],-15} |" +
                   // $" {rowView["group"],-15} |" +
                    $" {rowView["age"],-5}"
                );
            }
        }
    }
}