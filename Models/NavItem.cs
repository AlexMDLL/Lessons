namespace WpfApp26.Models
{
    // Пункт бокового меню.
    // Number — номер пункта меню, такой же, как в консольной версии:
    // 1 — Экипаж, 2 — Состояние систем, 3 — Проверить связь,
    // 4 — Сообщения станции, 5 — Полная диагностика, 6 — Аварийный режим.
    internal class NavItem
    {
        public SectionType Section { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public string Hint { get; set; }
    }
}
