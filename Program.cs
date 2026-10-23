using DatabaseCourseExam.AdoNet;
using System.Data;
using DatabaseCourseExam.EntityFramework;
using DatabaseCourseExam.DapperTasks;

DataTable? studentsTable = null;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Зачёт по работе с БД ===");
    Console.WriteLine("1. ADO.NET: Показать студентов");
    Console.WriteLine("2. ADO.NET: Изменить возраст студента");
    Console.WriteLine("3. ADO.NET: Записать студента на курс");
    Console.WriteLine("4. Отсоединенный режим: Загрузить студентов в DataTable");
    Console.WriteLine("5. Отсоединенный режим: Изменить возраст в DataTable");
    Console.WriteLine("6. Отсоединенный режим: Показать DataTable");
    Console.WriteLine("7. Отсоединенный режим: Сохранить изменения DataTable");
    Console.WriteLine("8. EF Core: Показать студентов");
    Console.WriteLine("9. EF Core: Изменить возраст студента");
    Console.WriteLine("10. EF Core: Показать курсы");
    Console.WriteLine("11. Dapper: Показать студентов");
    Console.WriteLine("12. Dapper: Изменить возраст");
    Console.WriteLine("13. Dapper: Показать курсы");
    Console.WriteLine("14. Dapper: Записать студента на курс");
    // TODO (задание 3): Добавить пункт меню, выводящий список студентов отдельными методами ADO.NET, EF Core и Dapper.
    Console.WriteLine("15.Показать студентов через ADO.NET, EF Core и Dapper");
    Console.WriteLine("0. Выход");
    Console.Write("Выберите действие: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await AdoNetTasks.ShowStudentsAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "2":
                Console.Write("ID студента: ");
                int studentId = int.Parse(Console.ReadLine()!);
                Console.Write("Новый возраст: ");
                int age = int.Parse(Console.ReadLine()!);
                await AdoNetTasks.UpdateStudentAgeAsync(studentId, age);
                Console.ReadLine();
                Console.Clear();
                break;

            case "3":
                Console.Write("ID студента: ");
                int enrollmentStudentId = int.Parse(Console.ReadLine()!);
                Console.Write("ID курса: ");
                int courseId = int.Parse(Console.ReadLine()!);
                await AdoNetTasks.EnrollStudentAsync(enrollmentStudentId, courseId);
                Console.ReadLine();
                Console.Clear();
                break;

            case "4":
                studentsTable = await DisconnectedTasks.LoadStudentsAsync();
                Console.WriteLine("Данные загружены в память.");
                DisconnectedTasks.PrintTable(studentsTable);
                Console.ReadLine();
                Console.Clear();
                break;

            case "5":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                Console.Write("ID студента: ");
                int disconnectedStudentId = int.Parse(Console.ReadLine()!);
                Console.Write("Новый возраст: ");
                int disconnectedAge = int.Parse(Console.ReadLine()!);

                if (disconnectedAge is < 14 or > 100)
                {
                    Console.WriteLine("Возраст должен быть от 14 до 100.");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                DisconnectedTasks.ChangeAgeInMemory(studentsTable, disconnectedStudentId, disconnectedAge);
                Console.ReadLine();
                Console.Clear();
                break;

            case "6":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                DisconnectedTasks.PrintTable(studentsTable);
                Console.ReadLine();
                Console.Clear();
                break;

            case "7":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                DisconnectedTasks.SaveChanges(studentsTable);
                Console.ReadLine();
                Console.Clear();
                break;

            case "8":
                await EntityFrameworkTasks.ShowStudentsAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "9":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int stId))
                {
                    Console.WriteLine("Некорректный ID.");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int newAge) || newAge is < 14 or > 100)
                {
                    Console.WriteLine("Возраст должен быть от 14 до 100.");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                await EntityFrameworkTasks.UpdateStudentAgeAsync(stId, newAge);
                Console.ReadLine();
                Console.Clear();
                break;

            case "10":
                await EntityFrameworkTasks.ShowCoursesAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "11":
                await DapperStudentTasks.ShowStudentsAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "12":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int dStudentId))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некорректный ID.");
                    Console.ResetColor();
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int dNewAge) || dNewAge is < 14 or > 100)
                {
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("Возраст должен быть от 14 до 100.");
                    Console.ResetColor();
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                await DapperStudentTasks.UpdateStudentAgeAsync(dStudentId, dNewAge);
                Console.ReadLine();
                Console.Clear();
                break;

            case "13":
                await DapperStudentTasks.ShowCoursesAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "14":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int dEnrollStudentId))
                {
                    Console.WriteLine("Некорректный ID.");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                Console.Write("ID курса: ");
                if (!int.TryParse(Console.ReadLine(), out int dCourseId))
                {
                    Console.WriteLine("Некорректный ID.");
                    Console.ReadLine();
                    Console.Clear();
                    break;
                }
                await DapperStudentTasks.EnrollStudentAsync(dEnrollStudentId, dCourseId);
                Console.ReadLine();
                Console.Clear();
                break;

            case "15":
                Console.WriteLine("");
                Console.WriteLine("хе-хе");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("1. ВЫВОД ЧЕРЕЗ ADO.NET (Connected)");
                Console.ResetColor();
                await AdoNetTasks.ShowStudentsAsync();
                Console.WriteLine("");
                Console.WriteLine("хе-хе");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("2. ВЫВОД ЧЕРЕЗ ENTITY FRAMEWORK CORE");
                Console.ResetColor();
                await EntityFrameworkTasks.ShowStudentsAsync();
                Console.WriteLine("");
                Console.WriteLine("хе-хе");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("3. ВЫВОД ЧЕРЕЗ DAPPER");
                Console.ResetColor();
                await DapperStudentTasks.ShowStudentsAsync();
                Console.ReadLine();
                Console.Clear();
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Не тот намбр");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка: {ex.Message}");
        Console.ResetColor();
    }
}