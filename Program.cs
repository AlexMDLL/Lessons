using System;
using Npgsql;
using System.Data;

namespace ConsoleApp11
{
    internal class Program
    {
        static void Main(String[] args)
        {
            DatabaseManager manager = new DatabaseManager();
            String connectionString = manager.GetConnectionString();

            Boolean exitRequested = false;

            while (!exitRequested)
            {
                Console.WriteLine("                                            ");
                Console.WriteLine("          МЕНЮ ПРОГРАММЫ");
                Console.WriteLine("                                            ");
                Console.WriteLine("1. Вставить нового студента");
                Console.WriteLine("2. Загрузить и вывести всех студентов");
                Console.WriteLine("3. Перенести все данные в личную базу");
                Console.WriteLine("4. Выход");
                Console.WriteLine("                                            ");
                Console.Write("Выберите действие: ");
                Console.WriteLine("                                            ");
                string choice = Console.ReadLine() ?? string.Empty;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine("\n--- Вставка данных ---");
                            manager.InsertStudentFromConsole(connectionString);
                            break;

                        case "2":
                            Console.WriteLine("\n--- Загрузка и вывод ---");
                            manager.LoadStudentsAndPrint(connectionString);
                            break;

                        case "3":
                            Console.WriteLine("\n--- Перенос данных в личную базу ---");
                            manager.CopyDataToPersonalDatabase();
                            break;

                        case "4":
                            Console.WriteLine("Выход из программы...");
                            exitRequested = true;
                            break;

                        default:
                            Console.WriteLine("Неверный выбор. Пожалуйста, введите 1, 2, 3 или 4");
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Произошла ошибка: {exception.Message}");
                }
            }
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}