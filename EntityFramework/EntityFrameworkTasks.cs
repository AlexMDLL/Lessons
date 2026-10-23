using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseCourseExam.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseCourseExam.EntityFramework;

public static class EntityFrameworkTasks
{
    // TODO (задание 12): Вывести студентов с названием группы; обеспечить загрузку связанной сущности без обращения к незагруженному свойству.
    // TODO (задание 11): Сгруппировать загруженных студентов по группе и вывести количество студентов в каждой группе.
    public static async Task ShowStudentsAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        // Include гарантирует, что Group будет загружен одним SQL-запросом
        List<Student> students = await context.Students
            .Include(student => student.Group)
            .OrderBy(student => student.Id)
            .ToListAsync();

        Console.WriteLine("--- Список студентов ---");
        foreach (Student student in students)
        {
            // Используем ?. и ?? для безопасной работы, если у студента нет группы
            string groupName = student.Group?.Name ?? "Без группы";
            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {groupName}");
        }

        // Группировка загруженных в память данных
        Console.WriteLine("\n--- Статистика по группам ---");
        var groupStats = students
            .GroupBy(s => s.Group?.Name ?? "Без группы")
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                GroupName = g.Key,
                Count = g.Count()
            });

        foreach (var stat in groupStats)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Группа '{stat.GroupName}': {stat.Count} студент(ов)");
            Console.ResetColor();
        }
    }

    // TODO (задание 6): Добавить обработку DbUpdateException и объяснить, почему ограничение уникальности должно обеспечиваться БД.
    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var context = SchoolDbContextFactory.Create();

        Student? student = await context.Students
            .FirstOrDefaultAsync(x => x.Id == studentId);

        if (student is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Студент не найден.");
            Console.ResetColor();
            return;
        }

        student.Age = newAge;

        try
        {
            int affectedRows = await context.SaveChangesAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Возраст изменён. Колличество сохраненых изменений: {affectedRows}");
            Console.ResetColor();
        }
        catch (DbUpdateException ex)
        {
            // DbUpdateException выбрасывается, когда база данных отклоняет операцию
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка обновления базы данных: {ex.Message}");
            Console.ResetColor();
        }
    }

    // TODO (задание 1): Выбирать только нужные поля через Select и добавить вычисляемый признак «мест нет».
    public static async Task ShowCoursesAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        //выбираем только нужные колонки
        var courses = await context.Courses
            .OrderBy(course => course.Name)
            .Select(course => new
            {
                course.Name,
                course.Price,
                course.AvailablePlaces,
                IsFull = course.AvailablePlaces == 0 // Вычисляемый призрак
            })
            .ToListAsync();

        foreach (var course in courses)
        {
            string status = course.IsFull ? " (Мест нет!)" : "";
            Console.WriteLine(
                $"{course.Name} | Цена: {course.Price:N2} | " +
                $"Свободных мест: {course.AvailablePlaces}{status}");
        }
    }
}