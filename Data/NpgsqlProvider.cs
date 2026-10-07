using System;
using System.IO;

namespace WpfApp26.Data
{
    // Провайдер PostgreSQL (Npgsql)
    internal class NpgsqlProvider : DbProvider
    {
        const string FileName = "db.local.txt";

        public override string Name
        {
            get { return "Npgsql"; }
        }

        public override string DisplayName
        {
            get { return "PostgreSQL"; }
        }

        // Строка подключения из db.local.txt (он в .gitignore)
        public override string GetConnectionString()
        {
            for (DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, FileName);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }

            throw new FileNotFoundException(
                "Не найден файл " + FileName + " со строкой подключения к БД. Образец — db.local.example.txt.");
        }

        public override string CodeFor(Exception ex)
        {
            Npgsql.PostgresException postgres = ex as Npgsql.PostgresException;
            return postgres == null ? string.Empty : " (" + postgres.SqlState + ")";
        }

        public override string HintFor(Exception ex)
        {
            Npgsql.PostgresException postgres = ex as Npgsql.PostgresException;
            if (postgres == null) return string.Empty;

            switch (postgres.SqlState)
            {
                case "28P01":
                    return "Похоже, пользователя ещё нет в базе или неверный пароль. Проверьте строку в db.local.txt.";

                case "42P01":
                    return "Таблицы нет: выполните скрипт db/lesson8_schema.sql.";

                case "3D000":
                    return "База данных не найдена — проверьте параметр Database в db.local.txt.";

                case "42501":
                    return "Недостаточно прав: нужны SELECT на crew и station_systems и UPDATE на crew.";

                default:
                    return "Код ошибки сервера: " + postgres.SqlState + ".";
            }
        }
    }
}
