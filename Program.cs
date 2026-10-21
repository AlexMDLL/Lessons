using ConsoleApp8.Models;
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
    bool isRunning = true;
    while (isRunning)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Урок 5 П1 ");
        Console.ResetColor();
        Console.WriteLine("1. Показать всех студентов");
        Console.WriteLine("2. Добавить студента");
        Console.WriteLine("3. Изменить возраст студента");
        Console.WriteLine("4. Удалить студента");
        Console.WriteLine("5. Перевести студента в другую группу");
        Console.WriteLine("0. Выход");
        Console.Write("\nВыберите действие: ");
        string? choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await ShowStudents(db);
                break;
            case "2":
                await AddStudent(db);
                break;
            case "3":
                await UpdateStudentAge(db);
                break;
            case "4":
                await DeleteStudent(db);
                break;
            case "5":
                await TransferStudent(db);
                break;
            case "0":
                isRunning = false;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Неверно. Нажмите чето");
                Console.ResetColor();
                Console.ReadLine();
                break;
        }
    }
    async Task ShowStudents(AppDbContext context)
    {
        List<Student> students = await context.Students
            .Include(s => s.Group)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        Console.WriteLine("\nСписок студентов");
        if (students.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Список пуст.");
            Console.ResetColor();
        }
        else
        {
            foreach (Student student in students)
            {
                string GrpN = student.Group?.Name ?? "Без группы";
                Console.WriteLine($"{student.Id}. {student.FirstName} {student.LastName}, Возраст: {student.Age}, Группа: {GrpN}");
            }
        }
        Console.WriteLine("Нажмите чето");
        Console.ReadLine();
    }

    async Task AddStudent(AppDbContext context)
    {
        Console.Write("Имя: ");
        string firstName = Console.ReadLine() ?? "";

        Console.Write("Фамилия: ");
        string lastName = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Ага, пусто значит, давай ты еще и возраст отрицательный укажешь?");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Console.Write("Возраст: ");
        if (!int.TryParse(Console.ReadLine(), out int age) || age <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Мммм отрицательный возраст, че смешно что ли?");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Console.Write("ID группы: ");
        if (!int.TryParse(Console.ReadLine(), out int groupId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Меняй формат ID умник.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        // Проверка существования группы перед добавлением
        bool GrpaEst = await context.Groups.AnyAsync(g => g.Id == groupId);
        if (!GrpaEst)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: NUH UH Группы со статиком {groupId} не сущесутвует, не фартануло");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Student newStudent = new Student
        {
            FirstName = firstName,
            LastName = lastName,
            Age = age,
            GroupId = groupId
        };

        context.Students.Add(newStudent);
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Студент был успешно забанен\nOй, точнее добавлен! \nСтатик: {newStudent.Id}");
        Console.ResetColor();
        Console.WriteLine("Нажмите чето");
        Console.ReadLine();
    }

    async Task UpdateStudentAge(AppDbContext context)
    {
        Console.Write("Введите ID студента для изменения: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Меняй формат ID умник.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Student? student = await context.Students.FindAsync(id);
        if (student == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Студента с таким ID нема.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Console.Write($"Текущий возраст: {student.Age}. Введите новый возраст: ");
        if (!int.TryParse(Console.ReadLine(), out int newAge) || newAge <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Мммм отрицательный возраст, че смешно что ли?");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        student.Age = newAge;
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Возраст обновлен.");
        Console.ResetColor();
        Console.WriteLine("Нажмите чето");
        Console.ReadLine();
    }

    async Task DeleteStudent(AppDbContext context)
    {
        Console.Write("Введите статик(ID) чтобы забанить студента: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Меняй формат ID умник.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Student? student = await context.Students.FindAsync(id);
        if (student == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Студента с таким ID нема.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        context.Students.Remove(student);
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("забанен успешно");
        Console.ResetColor();
        Console.WriteLine("Нажмите чето");
        Console.ReadLine();
    }
    async Task TransferStudent(AppDbContext context)
    {
        // Ввод ID студента
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("\nУбейте студента для перевода: ");
        Console.ResetColor();
        Console.ReadLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\nНе тот сценарий подождите");
        Console.ResetColor();
        Console.ReadLine();
        Console.Clear();
        Console.ReadLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Переписываю сценарий");
        Console.ResetColor();
        Console.ReadLine();
        Console.Clear();
        Console.ReadLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Готово!!");
        Console.ResetColor();
        Console.ReadLine();
        Console.Clear();
        Console.Write("\nВведите ID студента для перевода: ");
        if (!int.TryParse(Console.ReadLine(), out int studentId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: Неверный формат ID.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        // Поиск студента
        Student? student = await context.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Студента с таким ID нема.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"\nСтудент: {student.FirstName} {student.LastName}\n");
        Console.WriteLine($"Текущая группа: {student.Group?.Name ?? "Без группы"} (ID: {student.GroupId})\n");

        // Вывод доступных групп
        List<Group> groups = await context.Groups.OrderBy(g => g.Name).ToListAsync();

        if (groups.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("В базе нет групп для перевода.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        Console.WriteLine("\nДоступные группы:");
        foreach (Group group in groups)
        {
            Console.WriteLine($"ID: {group.Id} | Название: {group.Name}");
        }

        // Ввод ID новой группы
        Console.Write("\nВведите ID новой группы: ");
        if (!int.TryParse(Console.ReadLine(), out int newGroupId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ошибка: NUH UH формат группы неверный.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        // Проверка существования новой группы
        bool groupExists = await context.Groups.AnyAsync(g => g.Id == newGroupId);
        if (!groupExists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: Группы с ID {newGroupId} не существует.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }
        student.GroupId = newGroupId;

        // Сохраняем изменения в БД
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Студент переведенн в группу с ID {newGroupId}.");
        Console.ResetColor();
        Console.WriteLine("Нажмите чето");
        Console.ReadLine();
    }
    /*
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
        Console.Clear();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("База данных пуста, статистика недоступна.");
        Console.ResetColor();
    }
    */
}
catch
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Что то не то, удаляй VS:) <3");
    Console.ResetColor();
}