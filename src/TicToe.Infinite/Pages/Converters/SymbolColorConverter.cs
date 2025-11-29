using System.Globalization;

namespace TicToe.Infinite.Pages.Converters;

/// <summary>
/// 
/// </summary>
public class SymbolColorConverter : IValueConverter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is char c)
        {
            if (c == 'X') 
                return Color.FromArgb("#4EC9FF");

            if (c == 'O') 
                return Color.FromArgb("#FF66CC");
        }
        return Colors.White;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
