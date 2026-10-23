using System;
using System.Threading.Tasks;
using DatabaseCourseExam.Data;
using Dapper;
using DatabaseCourseExam.DapperTasks.DTO;

namespace DatabaseCourseExam.DapperTasks;

public static class DapperStudentTasks
{
    public static async Task ShowStudentsAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                s.id AS Id,
                s.first_name AS FirstName,
                s.last_name AS LastName,
                s.age AS Age,
                g.name AS GroupName
            FROM students AS s
            JOIN student_groups AS g ON g.id = s.group_id
            ORDER BY s.id;
            """;

        var students = await connection.QueryAsync<StudentDto>(sql);

        foreach (StudentDto student in students)
        {
            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {student.GroupName}");
        }
    }

    // TODO (задание 2): Проверить возраст и использовать число затронутых строк, чтобы отличить обновление от отсутствующего ID.
    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        // 1. Проверка валидности возраста до обращения к БД
        if (newAge <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Черт ты че, почему возраст отрицательный а?");
            Console.ResetColor();
            return;
        }

        await using var connection = DatabaseConnection.Create();

        const string sql = """
            UPDATE students
            SET age = @NewAge
            WHERE id = @StudentId;
            """;

        // 2. Выполнение запроса. ExecuteAsync возвращает количество реально изменённых строк
        int affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                NewAge = newAge,
                StudentId = studentId
            });

        // 3. Анализ результата (исправление инвертированной логики из исходного кода)
        if (affectedRows > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Возраст студента с ID {studentId} успешно изменён на {newAge}.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Студент с ID {studentId} не найден в базе данных.");
            Console.ResetColor();
        }
    }

    public static async Task ShowCoursesAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                id AS Id,
                name AS Name,
                price AS Price,
                available_places AS AvailablePlaces
            FROM courses
            ORDER BY name;
            """;

        var courses = await connection.QueryAsync<CourseDto>(sql);

        foreach (CourseDto course in courses)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"{course.Name} | Цена: {course.Price:N2} | " +
                $"Свободных мест: {course.AvailablePlaces}");
            Console.ResetColor();
        }
    }

    public static async Task EnrollStudentAsync(
        int studentId,
        int courseId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            const string updateSql = """
                UPDATE courses
                SET available_places = available_places - 1
                WHERE id = @CourseId
                  AND available_places > 0
                RETURNING available_places;
                """;

            int? remainingPlaces =
                await connection.QuerySingleOrDefaultAsync<int?>(
                    updateSql,
                    new { CourseId = courseId },
                    transaction);

            if (remainingPlaces is null)
            {
                throw new InvalidOperationException(
                    "Курс не найден или свободных мест нет.");
            }

            const string insertSql = """
                INSERT INTO student_courses (student_id, course_id)
                VALUES (@StudentId, @CourseId);
                """;

            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    StudentId = studentId,
                    CourseId = courseId
                },
                transaction);

            await transaction.CommitAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"Студент записан на курс. Осталось мест: {remainingPlaces}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Операция отменена: {ex.Message}");
            Console.ResetColor();
        }
    }
}