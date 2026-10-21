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

    // П1

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

    Console.WriteLine("\n\n П1 \n");

    if (LStud.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nСтуденты по заданным критериям не найдены.");
        Console.ResetColor();
    }
    else
    {
        foreach (var st in LStud)
        {
            Console.WriteLine($"{st.FirstName} {st.LastName}, {st.Age}");
        }
    }

    Console.WriteLine($"\nЕсть несовершеннолетние студенты: {HasM}\n");

    // П2
    Console.WriteLine("\n\n П2\n ");

    // Общее количество студентов
    var totalCount = await db.Students.CountAsync();
    Console.WriteLine($"\nВсего студентов: {totalCount}\n");

    if (totalCount > 0)
    {
        // Средний возраст
        var avgAge = await db.Students.AverageAsync(s => s.Age);
        Console.WriteLine($"\nСредний возраст: {avgAge:F1}\n");

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

        foreach (var item in ageStats)
        {
            Console.WriteLine($"{item.Age} лет — {item.Count}");
        }

        // Проверка: все ли студенты совершеннолетние
        var allAdults = await db.Students.AllAsync(s => s.Age >= 18);
        Console.WriteLine($"\nВсе студенты совершеннолетние: {allAdults}\n");
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