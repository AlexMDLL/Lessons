
using DatabaseCourseExam.Data;
using Npgsql;
using NpgsqlTypes;

namespace DatabaseCourseExam.AdoNet;

public static class AdoNetTasks
{
    // TODO (задание 7): Добавить отдельный запрос COUNT(*) для выбранной группы и корректно преобразовать результат ExecuteScalarAsync в int.
    // TODO (задание 10): Проверить потенциально NULL-значение через IsDBNull прежде чем вызывать GetString/GetInt32.
    public static async Task ShowStudentsAsync()
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            SELECT s.id, s.first_name, s.last_name, s.age, g.name
            FROM students AS s
            LEFT JOIN student_groups AS g ON g.id = s.group_id
            ORDER BY s.id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int id = reader.GetInt32(0);

            string firstName = reader.IsDBNull(1)
                ? "Не указано"
                : reader.GetString(1);

            string lastName = reader.IsDBNull(2)
                ? "Не указано"
                : reader.GetString(2);

            string age = reader.IsDBNull(3)
                ? "Не указан"
                : reader.GetInt32(3).ToString();

            string groupName = reader.IsDBNull(4)
                ? "Без группы"
                : reader.GetString(4);

            Console.WriteLine(
                $"{id}. {firstName} {lastName}, " +
                $"возраст: {age}, " +
                $"группа: {groupName}");
        }
    }

    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            UPDATE students
            SET age = @Age
            WHERE id = @StudentId;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add("@Age", NpgsqlDbType.Integer).Value = newAge;
        command.Parameters.Add("@StudentId", NpgsqlDbType.Integer).Value = studentId;

        int affectedRows = await command.ExecuteNonQueryAsync();

        Console.WriteLine(
            affectedRows == 0
                ? "Студент не найден."
                : $"Изменено записей: {affectedRows}");
    }

    public static async Task EnrollStudentAsync(
        int studentId,
        int courseId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string updateSql = """
                UPDATE courses
                SET available_places = available_places - 1
                WHERE id = @CourseId
                  AND available_places > 0
                RETURNING available_places;
                """;

            await using var updateCommand =
                new NpgsqlCommand(updateSql, connection, transaction);

            updateCommand.Parameters.Add(
                "@CourseId", NpgsqlDbType.Integer).Value = courseId;

            object? result = await updateCommand.ExecuteScalarAsync();

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Курс не найден или свободных мест нет.");
            }

            const string insertSql = """
                INSERT INTO student_courses (student_id, course_id)
                VALUES (@StudentId, @CourseId);
                """;

            await using var insertCommand =
                new NpgsqlCommand(insertSql, connection, transaction);

            insertCommand.Parameters.Add(
                "@StudentId", NpgsqlDbType.Integer).Value = studentId;

            insertCommand.Parameters.Add(
                "@CourseId", NpgsqlDbType.Integer).Value = courseId;

            await insertCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            Console.WriteLine("Студент записан на курс.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Операция отменена: {ex.Message}");
        }
    }

    public static async Task GetStudentCountByGroupAsync(int groupId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM students WHERE group_id = @GroupId;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", NpgsqlDbType.Integer).Value = groupId;

        // ExecuteScalarAsync возвращает object?, т.к тип данных неизвестен заранее
        object? result = await command.ExecuteScalarAsync();

        // COUNT(*) в PostgreSQL возвращает тип bigint
        // Прямое приведение может вызвать InvalidCastException.
        // Convert.ToInt32 преобразует long в int.
        int count = result != null ? Convert.ToInt32(result) : 0;

        Console.WriteLine($"Количество студентов в группе с ID {groupId}: {count}");
    }
}