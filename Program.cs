using System;
using System.Data;

namespace ConsoleApp9
{
    class Program
    {
        static DataTable studentTable;

        static void Main()
        {
            studentTable = new DataTable("Students");
            studentTable.Columns.Add("Id", typeof(int));
            studentTable.Columns.Add("Name", typeof(string));
            studentTable.Columns.Add("Age", typeof(int));
            studentTable.Columns.Add("GroupName", typeof(string));
            studentTable.PrimaryKey = new DataColumn[] { studentTable.Columns["Id"] };
            studentTable.Rows.Add(1, "Ivanov Ivan", 20, "CS-101");
            studentTable.Rows.Add(2, "Petrova Anna", 22, "CS-102");
            studentTable.Rows.Add(3, "Sidorov Alexey", 19, "CS-101");
            studentTable.Rows.Add(4, "Kozlova Maria", 21, "CS-103");
            studentTable.Rows.Add(5, "Novikov Dmitry", 23, "CS-102");
            studentTable.Rows.Add(6, "Fedorova Elena", 20, "CS-101");

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n=== Student Management System ===");
                Console.WriteLine("1. Show student table");
                Console.WriteLine("2. Find the oldest student");
                Console.WriteLine("3. Change student age by Id");
                Console.WriteLine("4. Add a new student");
                Console.WriteLine("5. Delete a student by Id");
                Console.WriteLine("0. Exit");
                Console.Write("\nSelect an action: ");

                string input = Console.ReadLine();
                Console.Clear();

                switch (input)
                {
                    case "1":
                        ShowTable();
                        break;

                    case "2":
                        FindOldestStudent();
                        break;

                    case "3":
                        EditStudentAge();
                        break;

                    case "4":
                        AddNewStudent();
                        break;

                    case "5":
                        DeleteStudent();
                        break;

                    case "0":
                        isRunning = false;
                        Console.WriteLine("Program terminated.");
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static void ShowTable()
        {
            Console.WriteLine("\n--- Student Table ---");
            PrintTable(studentTable);
        }

        static void FindOldestStudent()
        {
            DataRow oldestStudent = null;
            int maximumAge = -1;
            foreach (DataRow currentRow in studentTable.Rows)
            {
                int currentAge = (int)currentRow["Age"];
                if (currentAge > maximumAge)
                {
                    maximumAge = currentAge;
                    oldestStudent = currentRow;
                }
            }

            if (oldestStudent != null)
            {
                Console.WriteLine("\nThe oldest student is: " +
                    oldestStudent["Name"] + " (Age: " + maximumAge + ")");
            }
            else
            {
                Console.WriteLine("\nThe table is empty.");
            }
        }

        static void EditStudentAge()
        {
            Console.Write("\nEnter student Id to edit: ");
            string idInput = Console.ReadLine();

            if (int.TryParse(idInput, out int studentId))
            {
                DataRow studentToEdit = studentTable.Rows.Find(studentId);

                if (studentToEdit != null)
                {
                    Console.Write("Enter new age: ");
                    string ageInput = Console.ReadLine();

                    if (int.TryParse(ageInput, out int newAge))
                    {
                        studentToEdit["Age"] = newAge;
                        Console.WriteLine("Age for " + studentToEdit["Name"] +
                            " updated to " + newAge);
                    }
                    else
                    {
                        Console.WriteLine("Invalid age.");
                    }
                }
                else
                {
                    Console.WriteLine("Student with Id=" + studentId + " not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid Id.");
            }
        }

        static void AddNewStudent()
        {
            Console.Write("\nEnter Id: ");
            string idInput = Console.ReadLine();

            Console.Write("Enter name: ");
            string name = Console.ReadLine();

            Console.Write("Enter age: ");
            string ageInput = Console.ReadLine();

            Console.Write("Enter group name: ");
            string groupName = Console.ReadLine();

            if (int.TryParse(idInput, out int id) && int.TryParse(ageInput, out int age))
            {
                DataRow existingStudent = studentTable.Rows.Find(id);

                if (existingStudent == null)
                {
                    studentTable.Rows.Add(id, name, age, groupName);
                    Console.WriteLine("Student added successfully.");
                }
                else
                {
                    Console.WriteLine("Student with this Id already exists.");
                }
            }
            else
            {
                Console.WriteLine("Invalid Id or age.");
            }
        }

        static void DeleteStudent()
        {
            Console.Write("\nEnter student Id to delete: ");
            string idInput = Console.ReadLine();

            if (int.TryParse(idInput, out int studentId))
            {
                DataRow studentToDelete = studentTable.Rows.Find(studentId);

                if (studentToDelete != null)
                {
                    string studentName = studentToDelete["Name"].ToString();
                    studentToDelete.Delete();
                    studentTable.AcceptChanges();
                    Console.WriteLine("Student " + studentName + " deleted.");
                }
                else { Console.WriteLine("Student with Id=" + studentId + " not found."); }
            }
            else { Console.WriteLine("Invalid Id."); }
        }

        static void PrintTable(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                Console.Write(column.ColumnName.PadRight(15));
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 60));

            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    Console.Write(item.ToString().PadRight(15));
                }
                Console.WriteLine();
            }
        }
    }
}