using System;
using System.IO;

namespace ConsoleApp8
{
    // Реализация для провайдера Microsoft.Data.Sqlite
    internal class SqliteProvider : DbProvider
    {
        public override string Name
        {
            get { return "Microsoft.Data.Sqlite"; }
        }

        // База — обычный файл рядом с exe
        public override string GetConnectionString()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test01.db");
            return "Data Source=" + path;
        }
    }
}
