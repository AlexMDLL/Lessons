using System;
using System.IO;

namespace ConsoleApp8
{
    // Реализация для провайдера Npgsql
    internal class NpgsqlProvider : DbProvider
    {
        public override string Name
        {
            get { return "Npgsql"; }
        }

        // Строка подключения читается из файла db.local.txt рядом с Program.cs
        public override string GetConnectionString()
        {
            const string fileName = "db.local.txt";

            for (DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, fileName);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }

            throw new Exception("Не найден файл " + fileName + " со строкой подключения к БД.");
        }
    }
}
