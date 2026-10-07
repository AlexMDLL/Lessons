using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp26.Models
{
    // Член экипажа станции (таблица crew)
    internal class CrewMember : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int id;
        private string name;
        private string role;
        private int health;
        private string status;

        public int Id
        {
            get { return id; }
            set { id = value; N(); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; N(); N(nameof(Initials)); N(nameof(ShortName)); }
        }

        public string Role
        {
            get { return role; }
            set { role = value; N(); }
        }

        public int Health
        {
            get { return health; }
            set { health = value; N(); N(nameof(HealthText)); }
        }

        public string Status
        {
            get { return status; }
            set { status = value; N(); }
        }

        // "85%" для подписи рядом с полосой здоровья
        public string HealthText
        {
            get { return health + "%"; }
        }

        // Инициалы для аватарки в списке
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) return "??";
                string[] parts = name.Split(' ');
                if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
                return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpper();
            }
        }

        // Короткое имя "А. Волков"
        public string ShortName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) return string.Empty;
                string[] parts = name.Split(' ');
                return parts.Length == 1 ? parts[0] : parts[0].Substring(0, 1) + ". " + parts[1];
            }
        }

        public override string ToString()
        {
            return name;
        }
    }
}
