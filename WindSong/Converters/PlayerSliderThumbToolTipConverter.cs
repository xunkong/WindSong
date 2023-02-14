using Microsoft.UI.Xaml.Data;
using System;

namespace WindSong.Converters;

internal class PlayerSliderThumbToolTipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int or long or double)
        {
            var ms = System.Convert.ToDouble(value);
            var s = (int)(ms / 1000.0);
            return $"{s / 60:D2}:{s % 60:D2}";
        }
        else
        {
            return "";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
