namespace ConsoleApp14.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public bool IsActive { get; set; }
    }
}