namespace WpfApp26.Models
{
    // Сообщение, принятое со станции
    internal class StationMessage
    {
        public string Time { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public string Level { get; set; }   // Инфо / Внимание / Критично
    }
}
