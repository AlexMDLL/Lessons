using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp26.Models
{
    // Система станции (таблица station_systems)
    internal class StationSystem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int id;
        private string name;
        private string status;
        private string detail;
        private string lastCheck;

        public int Id
        {
            get { return id; }
            set { id = value; N(); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; N(); }
        }

        // OK / WARNING / CRITICAL
        public string Status
        {
            get { return status; }
            set { status = value; N(); }
        }

        public string Detail
        {
            get { return detail; }
            set { detail = value; N(); }
        }

        public string LastCheck
        {
            get { return lastCheck; }
            set { lastCheck = value; N(); }
        }
    }
}
