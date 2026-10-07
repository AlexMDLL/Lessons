using System;

namespace WpfApp26.Data
{
    // Общий предок провайдеров 
    internal abstract class DbProvider
    {
        public abstract string Name { get; }

        public abstract string GetConnectionString();

        // Имя для интерфейса: "PostgreSQL"
        public virtual string DisplayName
        {
            get { return Name; }
        }

        // Короткий код ошибки для строки состояния, например " (28P01)"
        public virtual string CodeFor(Exception ex)
        {
            return string.Empty;
        }

        // Подсказка, что делать с ошибкой (пустая строка — провайдеру добавить нечего)
        public virtual string HintFor(Exception ex)
        {
            return string.Empty;
        }
    }
}
