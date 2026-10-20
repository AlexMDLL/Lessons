using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder = Host.CreateApplicationBuilder(args);


string connectionString =
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Не найдена строка подключения Default.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

bool canConnect = await db.Database.CanConnectAsync();

Console.WriteLine(
    canConnect
        ? $"Подключение к PostgreSQL успешно!\n База данных: {db.Database.GetDbConnection().Database}"
        : "Не удалось подключиться к PostgreSQL.");
var students = await db.Students.ToListAsync();
Console.WriteLine("⁂ Студенты ⁂");
foreach(var st in students)
{
    Console.WriteLine($"{st.Id} | {st.FirstName} {st.LastName} | Возраст: {st.Age}");
}
Console.WriteLine($"\nВсего студентов: {students.Count}");
// Контрольные вопросы

Console.WriteLine("\n\nКОНТРОЛЬНЫЕ ВОПРОСЫ");
int correctAnswers = 0;
int totalQuestions = 4;
// Вопрос 1
Console.WriteLine("Вопрос 1: Какова основная задача DbContext?");
Console.WriteLine("1. Хранить все записи базы в памяти постоянно");
Console.WriteLine("2. Управлять взаимодействием приложения с EF Core и базой данных");
Console.WriteLine("3. Заменять PostgreSQL");
Console.Write("Ваш ответ (1-3): ");
string? answer1 = Console.ReadLine();
if (answer1 == "2")
{
    Console.WriteLine("✓ Правильно!\n");
    correctAnswers++;
}
else
{
    Console.WriteLine("✗ Вы выбрали неправильный ответ");
    Console.WriteLine("Правильный ответ: 2. Управлять взаимодействием приложения с EF Core и базой данных\n");
}

// Вопрос 2
Console.WriteLine("Вопрос 2: Что представляет собой DbSet?");
Console.WriteLine("1. Набор сущностей Student, доступный через контекст");
Console.WriteLine("2. Обычный SQL-файл");
Console.WriteLine("3. Строку подключения");
Console.Write("Ваш ответ (1-3): ");
string? answer2 = Console.ReadLine();
if (answer2 == "1")
{
    Console.WriteLine("✓ Правильно!\n");
    correctAnswers++;
}
else
{
    Console.WriteLine("✗ Вы выбрали неправильный ответ");
    Console.WriteLine("Правильный ответ: 1. Набор сущностей Student, доступный через контекст\n");
}

// Вопрос 3
Console.WriteLine("Вопрос 3: Что делает AddDbContext?");
Console.WriteLine("1. Немедленно создаёт таблицу в PostgreSQL");
Console.WriteLine("2. Регистрирует контекст в контейнере зависимостей");
Console.WriteLine("3. Добавляет тестовых студентов");
Console.Write("Ваш ответ (1-3): ");
string? answer3 = Console.ReadLine();
if (answer3 == "2")
{
    Console.WriteLine("✓ Правильно!\n");
    correctAnswers++;
}
else
{
    Console.WriteLine("✗ Вы выбрали неправильный ответ");
    Console.WriteLine("Правильный ответ: 2. Регистрирует контекст в контейнере зависимостей\n");
}

// Вопрос 4
Console.WriteLine("Вопрос 4: Когда выполняется запрос в выражении db.Students.ToListAsync()?");
Console.WriteLine("1. При создании класса Student");
Console.WriteLine("2. При материализации запроса через ToListAsync");
Console.WriteLine("3. При объявлении DbSet");
Console.Write("Ваш ответ (1-3): ");
string? answer4 = Console.ReadLine();
if (answer4 == "2")
{
    Console.WriteLine("✓ Правильно!\n");
    correctAnswers++;
}
else
{
    Console.WriteLine("✗ Вы выбрали неправильный ответ");
    Console.WriteLine("Правильный ответ: 2. При материализации запроса через ToListAsync\n");
}

// Итоговый результат
Console.WriteLine($"РЕЗУЛЬТАТ: {correctAnswers} из {totalQuestions} правильных ответов");

if (correctAnswers == totalQuestions)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Отлично! Вы ответили на все вопросы правильно!");
    Console.ResetColor();
}
else if (correctAnswers >= totalQuestions / 2)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Хороший результат, но есть что повторить.");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Рекомендуется перечитать материал урока.");
    Console.ResetColor();
}