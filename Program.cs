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
            DataTable studentsTable = new DataTable("Students");
            studentsTable.Columns.Add("StudentId", typeof(int));
            studentsTable.Columns.Add("StudentName", typeof(string));
            studentsTable.Columns.Add("Age", typeof(int));
            studentsTable.Columns.Add("GroupId", typeof(int));
            studentsTable.PrimaryKey = new DataColumn[] { studentsTable.Columns["StudentId"] };
            groupsTable.Rows.Add(1, "CS-101");
            groupsTable.Rows.Add(2, "CS-102");
            groupsTable.Rows.Add(3, "Math-201");
            studentsTable.Rows.Add(1, "Ivanov Ivan", 20, 1);
            studentsTable.Rows.Add(2, "Petrova Anna", 22, 1);
            studentsTable.Rows.Add(3, "Sidorov Alexey", 19, 1);
            studentsTable.Rows.Add(4, "Kozlova Maria", 21, 2);
            studentsTable.Rows.Add(5, "Novikov Dmitry", 23, 2);
            studentsTable.Rows.Add(6, "Fedorova Elena", 20, 2);
            studentsTable.Rows.Add(7, "Smirnov Oleg", 19, 3);
            studentsTable.Rows.Add(8, "Volkova Olga", 20, 3);
            studentsTable.Rows.Add(9, "Lebedev Maxim", 21, 3);
            studentsTable.Rows.Add(10, "Sokolova Daria", 22, 3);
            universityDataSet.Tables.Add(groupsTable);
            universityDataSet.Tables.Add(studentsTable);
            DataRelation groupStudentRelation = new DataRelation( "GroupStudentsRelation", groupsTable.Columns["GroupId"], studentsTable.Columns["GroupId"] );
            universityDataSet.Relations.Add(groupStudentRelation);
            int targetGroupId = 2;
            DataRow targetGroup = groupsTable.Rows.Find(targetGroupId);
            if (targetGroup != null)
            {
                Console.WriteLine("Students in group: " + targetGroup["GroupName"]);
                Console.WriteLine(new string('-', 30));
                DataRow[] groupStudents = targetGroup.GetChildRows(groupStudentRelation);

                foreach (DataRow student in groupStudents)
                {
                    Console.WriteLine("Name: " + student["StudentName"] + ", Age: " + student["Age"]);
                }
            }
            else { Console.WriteLine("Group not found."); }
        }
    }
}
