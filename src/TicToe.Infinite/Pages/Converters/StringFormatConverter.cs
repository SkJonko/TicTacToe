using System.Globalization;

namespace TicToe.Infinite.Pages.Converters;

/// <summary>
/// Converter that converts a base string with format placeholders and applies the provided arguments. 
/// </summary>
public class StringFormatConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts a base string with format placeholders and applies the provided arguments. 
    /// </summary>
    /// <param name="values">The first attribute must be always the value with the placeholders that you want to replace.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
		if (values == null || values.Length == 0)
		{
			return string.Empty;
		}

		// First value MUST be the message from .resx
		string baseMessage = values[0]?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(baseMessage))
		{
			return string.Empty;
		}

		// Remaining bindings are format arguments
		object[] formatArgs = new object[values.Length - 1];

        for (int i = 1; i < values.Length; i++)
		{
			formatArgs[i - 1] = values[i];
		}

		try
        {
            return string.Format(baseMessage, formatArgs);
        }
        catch
        {
            return baseMessage;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
