using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

var builder = Host.CreateApplicationBuilder(args);
string connStr = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Не найдена строка подключения Default.");
builder.Services.AddDbContext<AppDbContext>(opt => opt .UseNpgsql(connStr));

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

try
{
    //Урок 4 П1
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Урок 4 П1 \n");
    Console.ResetColor();

    var StudWhGrp = await db.Students
        .Include(s => s.Group)
        .OrderBy(s => s.LastName)
        .ThenBy(s => s.FirstName)
        .ToListAsync();

    if (StudWhGrp.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Список студентов пуст.");
        Console.ResetColor();
    }
    else { foreach (var st in StudWhGrp) { 
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\n{st.FirstName} {st.LastName} — Группа: {st.Group.Name}");
            Console.ResetColor();
        }
    }
    Console.ReadLine();
    Console.Beep();
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Урок 4 П2 \n");
    Console.ResetColor();

    // Запрос начинается с Groups, чтобы не потерять пустые группы.
    var GrpSt = await db.Groups
        .OrderBy(g => g.Name)
        .Select(g => new
        {
            GroupName = g.Name,
            StudentCount = g.Students.Count(),
            MinorCount = g.Students.Count(s => s.Age < 18),
            AverageAge = g.Students.Select(s => (double?)s.Age).Average(),
            HasMinors = g.Students.Any(s => s.Age < 18)
        })
        .ToListAsync();

    if (GrpSt.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Группы не найдены.");
        Console.ResetColor();
    }
    else
    {
        foreach (var i in GrpSt)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\nГруппа: {i.GroupName}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Студентов: {i.StudentCount}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Несовершеннолетних: {i.MinorCount}");
            Console.ResetColor();

            if (i.AverageAge.HasValue) { 
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"Средний возраст: {i.AverageAge.Value:f0}"); 
                Console.ResetColor();
            }
            else {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Средний возраст: нет данных (группа пуста)");
                Console.ResetColor();
            }

            Console.WriteLine(i.HasMinors ? "Есть несовершеннолетние: Да" : "Есть несовершеннолетние: Нет");
        }
    }

    // Урок 3 П1
    Console.ReadLine();
    Console.Beep();
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Урок 3 П1\n ");
    Console.ResetColor();
    Console.Write("\nМинимальный возраст: ");
    string? minInp = Console.ReadLine();
    if (!int.TryParse(minInp, out int minA))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка: введен некорректный возраст.");
        Console.ResetColor();
        return;
    }

    Console.Write("\nМаксимальный возраст: ");
    string? maxInp = Console.ReadLine();
    if (!int.TryParse(maxInp, out int maxA))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка: введен некорректный возраст.");
        Console.ResetColor();
        return;
    }

    Console.Write("\nНомер страницы: ");
    string? pageInp = Console.ReadLine();
    if (!int.TryParse(pageInp, out int pg) || pg < 1)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка: номер страницы должен быть больше 0.");
        Console.ResetColor();
        return;
    }

    const int pageSize = 3;

    //Проверка наличия несовершеннолетних
    var HasM = await db.Students.AnyAsync(s => s.Age < 18);

    var LStud = await db.Students
        .Where(s => s.Age >= minA && s.Age <= maxA)
        .OrderBy(s => s.LastName)
        .ThenBy(s => s.FirstName)
        .Select(s => new
        {
            s.FirstName,
            s.LastName,
            s.Age
        })
        .Skip((pg - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    if (LStud.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nСтуденты по заданным критериям не найдены.");
        Console.ResetColor();
    }
    else {  foreach (var st in LStud) { Console.WriteLine($"\n{st.FirstName} {st.LastName}, {st.Age}"); } }
    Console.WriteLine($"\nЕсть несовершеннолетние студенты: {HasM}\n");

    //Урок 3 П2
    Console.ReadLine();
    Console.Beep();
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Урок 3 П2\n ");
    Console.ResetColor();

    // Общее количество студентов
    var totalCount = await db.Students.CountAsync();
    Console.WriteLine($"\nВсего студентов: {totalCount}\n");

    if (totalCount > 0)
    {
        // Средний возраст
        var avgAge = await db.Students.AverageAsync(s => s.Age);
        Console.WriteLine($"\nСредний возраст: {avgAge:f0}\n");

        // Минимальный и максимальный возраст
        var minAgeVal = await db.Students.MinAsync(s => s.Age);
        var maxAgeVal = await db.Students.MaxAsync(s => s.Age);
        Console.WriteLine($"\nМинимальный возраст: {minAgeVal}\n");
        Console.WriteLine($"\nМаксимальный возраст: {maxAgeVal}\n");

        // Группировка по возрасту
        Console.WriteLine("\nРаспределение по возрасту:\n");
        var ageStats = await db.Students
            .GroupBy(s => s.Age)
            .Select(g => new
            {
                Age = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Age)
            .ToListAsync();

        foreach (var i in ageStats) { Console.WriteLine($"{i.Age} лет — {i.Count}"); }
        // Проверка: все ли студенты совершеннолетние
        var allAdults = await db.Students.AllAsync(s => s.Age >= 18);
        Console.WriteLine($"\nВсе студенты совершеннолетние: {allAdults}\n");
        Console.ReadLine();
        Console.Beep();
        Console.Clear();
        Console.ReadLine();
        Console.Beep();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("База данных пуста, статистика недоступна.");
        Console.ResetColor();
    }
}
catch
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Что то не то, удаляй VS:) <3");
    Console.ResetColor();
}
finally { Console.ReadKey(); }