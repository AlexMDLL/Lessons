using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp10
{
    class Program
    {
        static void Main()
        {
            DataSet universityDataSet = new DataSet("University");
            DataTable groupsTable = new DataTable("Groups");
            groupsTable.Columns.Add("GroupId", typeof(int));
            groupsTable.Columns.Add("GroupName", typeof(string));
            groupsTable.PrimaryKey = new DataColumn[] { groupsTable.Columns["GroupId"] };
            groupsTable.ColumnChanging += GroupsTable_ColumnChanging;
            DataTable studentsTable = new DataTable("Students");
            studentsTable.Columns.Add("StudentId", typeof(int));
            studentsTable.Columns.Add("StudentName", typeof(string));
            studentsTable.Columns.Add("GroupId", typeof(int));
            studentsTable.PrimaryKey = new DataColumn[] { studentsTable.Columns["StudentId"] };
            universityDataSet.Tables.Add(groupsTable);
            universityDataSet.Tables.Add(studentsTable);
            DataRelation groupStudentRelation = new DataRelation("GroupStudents", groupsTable.Columns["GroupId"], studentsTable.Columns["GroupId"], createConstraints: false);
            universityDataSet.Relations.Add(groupStudentRelation);
            groupsTable.Rows.Add(1, "ИС-101");
            groupsTable.Rows.Add(2, "ИС-102");
            groupsTable.Rows.Add(3, "CS-2");
            studentsTable.Rows.Add(1, "Иванов Иван", 1);
            studentsTable.Rows.Add(2, "Петрова Анна", 1);
            studentsTable.Rows.Add(3, "Сидоров Алексей", 1);
            studentsTable.Rows.Add(4, "Козлова Мария", 2);
            studentsTable.Rows.Add(5, "Новиков Дмитрий", 2);
            studentsTable.Rows.Add(6, "Федорова Елена", 2);
            studentsTable.Rows.Add(7, "Смирнов Олег", 3);
            studentsTable.Rows.Add(8, "Волкова Ольга", 3);
            studentsTable.Rows.Add(9, "Лебедев Максим", 3);
            studentsTable.Rows.Add(10, "Соколова Дарья", 99);
            Console.WriteLine("Нажмайте любую клавишу");
            Console.ReadLine();
            
            Console.WriteLine("Задание: Дубликат первичного ключа");
            try
            {
                groupsTable.Rows.Add(1, "Дублирующая группа");
                Console.WriteLine("Запись добавлена.");
                Console.ReadLine();
            }

            catch (Exception ex) { Console.WriteLine("Ошибка целостности: " + ex.Message); }
            Console.WriteLine("\nЗадание:  Пустое название группы");
            try
            {
                DataRow newGroup = groupsTable.NewRow();
                newGroup["GroupId"] = 4;
                newGroup["GroupName"] = "";
                groupsTable.Rows.Add(newGroup);
                Console.WriteLine("Запись добавлена.");
            }
            catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
            Console.WriteLine("\nЗадание:  Студенты без группы ");
            FindOrphanStudents(studentsTable, groupsTable, groupStudentRelation);
            Console.ReadLine();
        }
        static void GroupsTable_ColumnChanging(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName == "GroupName")
            {
                string value = e.ProposedValue as string;
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("Отмена изменения: название группы не может быть пустым.");
                    e.Row.RejectChanges();
                    Console.ReadLine();
                    throw new Exception("Название группы не может быть пустым.");

                }
            }
        }
        static void FindOrphanStudents(DataTable studentsTable, DataTable groupsTable, DataRelation relation)
        {
            int orphanCount = 0;

            foreach (DataRow student in studentsTable.Rows)
            {
                DataRow parentGroup = student.GetParentRow(relation);

                if (parentGroup == null)
                {
                    Console.WriteLine("Студент \"" + student["StudentName"] + "\" (Id=" + student["StudentId"] + ") ссылается на несуществующую группу GroupId=" + student["GroupId"]);
                    orphanCount++;
                    Console.ReadLine();
                }
            }
            if (orphanCount == 0) { Console.WriteLine("Все студенты привязаны к существующим группам."); }
            else { Console.WriteLine("\nВсего найдено студентов без группы: " + orphanCount); }
        }
    }
}