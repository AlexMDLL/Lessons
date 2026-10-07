using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp26.Infrastructure
{
    // Цвет по значению здоровья 0..100: >=70 зелёный, >=40 жёлтый, иначе красный
    internal class HealthToBrushConverter : IValueConverter
    {
        static readonly Brush Success = Freeze(new SolidColorBrush(Color.FromRgb(0x3D, 0xDC, 0x97)));
        static readonly Brush Warning = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0xC2, 0x4B)));
        static readonly Brush Danger = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0x5C, 0x7A)));

        static Brush Freeze(Brush brush)
        {
            brush.Freeze();
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int health = value is int ? (int)value : 0;
            if (health >= 70) return Success;
            if (health >= 40) return Warning;
            return Danger;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // Цвет по текстовому статусу: OK / WARNING / CRITICAL и русские подписи
    internal class StatusToBrushConverter : IValueConverter
    {
        static readonly Brush Success = Freeze(new SolidColorBrush(Color.FromRgb(0x3D, 0xDC, 0x97)));
        static readonly Brush Warning = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0xC2, 0x4B)));
        static readonly Brush Danger = Freeze(new SolidColorBrush(Color.FromRgb(0xFF, 0x5C, 0x7A)));
        static readonly Brush Accent = Freeze(new SolidColorBrush(Color.FromRgb(0xA5, 0x7B, 0xFF)));
        static readonly Brush Dim = Freeze(new SolidColorBrush(Color.FromRgb(0xB0, 0xA6, 0xC6)));

        static Brush Freeze(Brush brush)
        {
            brush.Freeze();
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = (value as string ?? string.Empty).Trim().ToLowerInvariant();

            if (status.Contains("снят") || status.Contains("штатн") || status.Contains("работоспособ")
                || status.Contains("норм") || status.Contains("ok") || status.Contains("станции")
                || status.Contains("успеш") || status.Contains("готов") || status.Contains("устойчив"))
                return Success;

            if (status.Contains("critical") || status.Contains("авар") || status.Contains("эвакуа")
                || status.Contains("критич") || status.Contains("поврежд"))
                return Danger;

            if (status.Contains("warning") || status.Contains("вниман") || status.Contains("отклоне")
                || status.Contains("медиц") || status.Contains("ремонт"))
                return Warning;

            if (status.Contains("инфо") || status.Contains("космос") || status.Contains("работ"))
                return Accent;

            return Dim;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // bool -> Visibility (с возможностью инвертировать)
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool && (bool)value;
            if (Invert) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is Visibility && (Visibility)value == Visibility.Visible;
            return Invert ? !visible : visible;
        }
    }
}
