using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Converters;

// One way converter
public class BusyStateToIndicatorColorConverter : IValueConverter
{

    public Color BusyColor { get; set; }
    public Color ReadyColor { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isBusy = (bool)value;

        return isBusy ? BusyColor : ReadyColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}