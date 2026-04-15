using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanzas_Faciles.Helpers
{
    public class EstadoToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool tieneExcedente)
                return null;

            var key = tieneExcedente ? "SuccessLight" : "WarningOrange";
            return Application.Current?.Resources[key];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
