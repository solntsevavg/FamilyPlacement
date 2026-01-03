using FamilyPlacement.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FamilyPlacement.ViewModels
{
    internal class TreeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeType type)
            {
                switch (type)
                {
                    case TreeType.Oak: return "Дуб";
                    case TreeType.Birch: return "Береза";
                    case TreeType.Pine: return "Сосна";
                    case TreeType.Spruce: return "Ель";
                    default: return value.ToString();
                }
            }
            return value?.ToString() ?? string.Empty;
        }
                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                switch (str)
                {
                    case "Дуб": return TreeType.Oak;
                    case "Береза": return TreeType.Birch;
                    case "Сосна": return TreeType.Pine;
                    case "Ель": return TreeType.Spruce;
                    default:
                        throw new ArgumentException($"Неизвестный тип дерева: {str}");
                }
            }
            throw new ArgumentException("Некорректное значение для конвертации");
        }
    }
}
