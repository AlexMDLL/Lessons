//using ConsoleApp14.Models;
//// Задача 1
//Student student = new Student
//{
//    FirstName = "Анна",
//    LastName = "Петрова",
//    Age = 21,
//    Email = "anna@example.com",
//    EnrollmentYear = 2025,
//    IsActive = true
//};

//Console.WriteLine("⁂ Исходные данные ⁂");
//Console.WriteLine($"Студент: {student.FirstName} {student.LastName}");
//Console.WriteLine($"Возраст: {student.Age}");
//Console.WriteLine($"Email: {student.Email}");
//Console.WriteLine($"Год поступления: {student.EnrollmentYear}");

//student.LastName = "Сидорова";
//student.Age = 22;
//student.EnrollmentYear = 1488;

//Console.WriteLine("\n⁂ После изменения ⁂");

//Console.Write("Студент: ");
//Console.Write(student.FirstName + " ");
//Console.ForegroundColor = ConsoleColor.Magenta;
//Console.Write(student.LastName);
//Console.ResetColor();
//Console.WriteLine();

//Console.Write("Возраст: ");
//Console.ForegroundColor = ConsoleColor.Magenta;
//Console.Write(student.Age);
//Console.ResetColor();
//Console.WriteLine();

//Console.WriteLine($"Email: {student.Email}");

//Console.Write("Год поступления: ");
//Console.ForegroundColor = ConsoleColor.Magenta;
//Console.Write(student.EnrollmentYear);
//Console.ResetColor();
//Console.WriteLine();



//// Задача 2 

//Console.WriteLine("\nЗАДАЧА 2");


//List<Student> studentsList = new List<Student>
//{
//    new Student
//    {
//        FirstName = "Никита",
//        LastName = "Некротов",
//        Age = 20,
//        Email = "ivan@example.com",
//        EnrollmentYear = 2024,
//        IsActive = true
//    },
//    new Student
//    {
//        FirstName = "Ирина",
//        LastName = "Админова",
//        Age = 21,
//        Email = "anna@example.com",
//        EnrollmentYear = 2025,
//        IsActive = true
//    },
//    new Student
//    {
//        FirstName = "Сергей",
//        LastName = "Петухов",
//        Age = 19,
//        Email = "sergey@example.com",
//        EnrollmentYear = 2025,
//        IsActive = false
//    }
//};

//Console.WriteLine("\n⁂ Активные студенты ⁂");
//IEnumerable<Student> activeStudents = studentsList.Where(s => s.IsActive);

//foreach (Student currentStudent in activeStudents)
//{
//    Console.WriteLine($"{currentStudent.FirstName} {currentStudent.LastName}, Возраст: {currentStudent.Age}");
//}

//int totalCount = studentsList.Count;
//double averageAge = studentsList.Average(s => s.Age);

//Console.WriteLine($"\nВсего студентов: {totalCount}");
//Console.WriteLine($"Средний возраст: {averageAge:f0}"); 
