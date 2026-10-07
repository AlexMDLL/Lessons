using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfApp26.Models;

namespace WpfApp26.Data
{
    // Запросы к базе: всё выполняется асинхронно
    internal static class StationRepository
    {
        public static async Task<List<CrewMember>> GetCrewAsync()
        {
            const string sql = "SELECT id, name, role, health, status FROM crew ORDER BY id;";

            List<CrewMember> crew = new List<CrewMember>();

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    crew.Add(ReadCrewMember(reader));
                }
            }

            return crew;
        }

        public static async Task<List<CrewMember>> FindCrewAsync(string name)
        {
            const string sql = "SELECT id, name, role, health, status FROM crew WHERE name ILIKE @name ORDER BY id;";

            List<CrewMember> crew = new List<CrewMember>();

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                Db.AddParameter(command, "@name", "%" + name + "%");

                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        crew.Add(ReadCrewMember(reader));
                    }
                }
            }

            return crew;
        }

        // Одна запись по идентификатору — чтобы показать в журнале значение из базы
        public static async Task<CrewMember> GetCrewMemberAsync(int id)
        {
            const string sql = "SELECT id, name, role, health, status FROM crew WHERE id = @id;";

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                Db.AddParameter(command, "@id", id);

                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadCrewMember(reader);
                    }
                }
            }

            return null;
        }

        public static async Task<int> DamageAsync(int id, int damage)
        {
            const string sql = "UPDATE crew SET health = GREATEST(health - @damage, 0) WHERE id = @id;";

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                Db.AddParameter(command, "@damage", damage);
                Db.AddParameter(command, "@id", id);

                return await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> HealAsync(int id, int heal)
        {
            const string sql = "UPDATE crew SET health = LEAST(health + @heal, 100) WHERE id = @id;";

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                Db.AddParameter(command, "@heal", heal);
                Db.AddParameter(command, "@id", id);

                return await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE crew SET status = @status WHERE id = @id;";

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                Db.AddParameter(command, "@status", status);
                Db.AddParameter(command, "@id", id);

                return await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<List<StationSystem>> GetSystemsAsync()
        {
            const string sql = "SELECT id, name, status FROM station_systems ORDER BY id;";

            List<StationSystem> systems = new List<StationSystem>();

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string status = reader.IsDBNull(2) ? "OK" : reader.GetString(2);

                    systems.Add(new StationSystem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Status = status,
                        Detail = DescribeStatus(status),
                        LastCheck = "из базы"
                    });
                }
            }

            return systems;
        }

        public static async Task<long> PingAsync()
        {
            const string sql = "SELECT now();";

            Stopwatch watch = Stopwatch.StartNew();

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                await command.ExecuteScalarAsync();
            }

            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        public static async Task<DateTime> GetServerTimeAsync()
        {
            const string sql = "SELECT now();";

            using (DbConnection connection = await Db.OpenConnectionAsync())
            using (DbCommand command = Db.CreateCommand(connection, sql))
            {
                object value = await command.ExecuteScalarAsync();
                return value is DateTime ? (DateTime)value : DateTime.Now;
            }
        }

        static CrewMember ReadCrewMember(DbDataReader reader)
        {
            return new CrewMember
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Role = reader.GetString(2),
                Health = reader.GetInt32(3),
                Status = reader.GetString(4)
            };
        }

        // В таблице station_systems есть только статус, поэтому пояснение
        static string DescribeStatus(string status)
        {
            if (string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
                return "отклонений нет, параметры в норме";

            if (string.Equals(status, "WARNING", StringComparison.OrdinalIgnoreCase))
                return "параметры на границе нормы, нужен контроль";

            return "требуется вмешательство оператора";
        }
    }
}
