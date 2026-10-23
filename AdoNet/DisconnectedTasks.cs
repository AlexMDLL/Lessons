using System.Data;
using DatabaseCourseExam.Data;
using Npgsql;

namespace DatabaseCourseExam.AdoNet;

public static class DisconnectedTasks
{
    public static async Task<DataTable> LoadStudentsAsync()
    {
        var table = new DataTable("Students");

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, first_name, last_name, age, group_id
            FROM students
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        using var adapter = new NpgsqlDataAdapter(command);

        adapter.Fill(table);
        table.PrimaryKey = [table.Columns["id"]!];

        return table;
    }

    // TODO (задание 5): Загрузить таблицы студентов и групп в DataSet и создать DataRelation
    public static async Task<DataSet> LoadStudentsAndGroupsAsync()
    {
        var dataSet = new DataSet();

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        // 1. Загружаем таблицу группы
        using var groupsAdapter = new NpgsqlDataAdapter("SELECT id, name FROM student_groups ORDER BY id", connection);
        groupsAdapter.Fill(dataSet, "Groups");
        dataSet.Tables["Groups"]!.PrimaryKey = [dataSet.Tables["Groups"]!.Columns["id"]!];

        // 2. Загружаем таблицу студенты
        using var studentsAdapter = new NpgsqlDataAdapter("SELECT id, first_name, last_name, age, group_id FROM students ORDER BY id", connection);
        studentsAdapter.Fill(dataSet, "Students");
        dataSet.Tables["Students"]!.PrimaryKey = [dataSet.Tables["Students"]!.Columns["id"]!];

        // 3. Создаём связь DataRelation
        var relation = new DataRelation(
            "FK_Students_Groups",
            parentColumn: dataSet.Tables["Groups"]!.Columns["id"],
            childColumn: dataSet.Tables["Students"]!.Columns["group_id"]
        );

        dataSet.Relations.Add(relation);

        return dataSet;
    }

    public static void PrintTable(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine(
                $"{row["id"]}: {row["first_name"]} " +
                $"{row["last_name"]}, возраст {row["age"]}");
        }
    }

    // TODO (задание 8): Реализовать добавление новой строки в DataTable с проверкой возраста и правильным RowState Added.
    public static void AddStudentToTable(DataTable table, string firstName, string lastName, int age, int groupId)
    {
        if (age <= 0)
        {
            throw new ArgumentException("Черт ты че, почему возраст отрицательный а?");
        }

        // Создаем новую строку, привязанную к схеме таблицы
        DataRow newRow = table.NewRow();

        newRow["first_name"] = firstName;
        newRow["last_name"] = lastName;
        newRow["age"] = age;
        newRow["group_id"] = groupId;

        // Добавление Rows автоматически меняет RowState на Added
        table.Rows.Add(newRow);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Студент {firstName} {lastName} добавлен в таблицу. RowState: {newRow.RowState}");
        Console.ResetColor();
    }

    // TODO (задание 9): Найти строку по первичному ключу, обработать отсутствие результата и не менять строку, если значение не изменилось.
    public static void ChangeAgeInMemory(DataTable table, int studentId, int newAge)
    {
        if (newAge <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Черт ты че, почему возраст отрицательный а?");
            Console.ResetColor();
            return;
        }

        DataRow? row = table.Rows.Find(studentId);

        if (row is null)
        {
            Console.WriteLine("Студент не найден в DataTable.");
            return;
        }

        // Проверяем, действительно ли значение изменилось
        int currentAge = Convert.ToInt32(row["age"]);
        if (currentAge == newAge)
        {
            Console.WriteLine("Возраст не изменился.");
            return;
        }

        // Изменяем значение только если оно новое
        row["age"] = newAge;

        Console.WriteLine($"Возраст изменён в памяти. Состояние строки: {row.RowState}");
    }

    // TODO (задание 4): После Update вывести количество сохранённых строк и проверить, остались ли строки в состоянии Modified.
    public static int SaveChanges(DataTable table)
    {
        using var connection = DatabaseConnection.Create();
        connection.Open();
        const string updateSql = """
        UPDATE students
        SET age = @age
        WHERE id = @id;
        """;
        using var updateCommand = new NpgsqlCommand(updateSql, connection);
        updateCommand.Parameters.Add("@age", NpgsqlTypes.NpgsqlDbType.Integer, 4, "age");
        updateCommand.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "id");

        const string insertSql = "INSERT INTO students (first_name, last_name, age, group_id) VALUES (@first_name, @last_name, @age, @group_id);";
        using var insertCommand = new NpgsqlCommand(insertSql, connection);
        insertCommand.Parameters.Add("@first_name", NpgsqlTypes.NpgsqlDbType.Varchar, 100, "first_name");
        insertCommand.Parameters.Add("@last_name", NpgsqlTypes.NpgsqlDbType.Varchar, 100, "last_name");
        insertCommand.Parameters.Add("@age", NpgsqlTypes.NpgsqlDbType.Integer, 4, "age");
        insertCommand.Parameters.Add("@group_id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "group_id");

        using var adapter = new NpgsqlDataAdapter();
        adapter.UpdateCommand = updateCommand;
        adapter.InsertCommand = insertCommand;

        // Update проходит по всем строкам и выполняет нужную команду (Insert/Update/Delete)
        int affectedRows = adapter.Update(table);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Сохранено изменений в БД: {affectedRows}");
        Console.ResetColor();

        bool hModRows = table.AsEnumerable().Any(r => r.RowState == DataRowState.Modified);

        if (hModRows)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("В таблице остались строки Modified");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Проверка пройдена: строк Modified не осталось");
        }

        return affectedRows;
    }
}