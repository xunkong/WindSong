using Microsoft.UI.Xaml.Data;
using System;

namespace WindSong.Converters;

internal class PlayerSliderThumbToolTipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int or double)
        {
            return $"{(int)value / 60:D2}:{(int)value % 60:D2}";
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
