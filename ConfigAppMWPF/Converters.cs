using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ConfigApp
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RowSplitConverter : IValueConverter
    {
        private const int MaxRow = 9;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showTwoRow && parameter is string p)
            {
                /* False > One row
                 * True > Two row
                 * Parameter expected > [item index],[r/c (Row/Column)]
                 * */
                var parameters = p.Split(',');
                int index = int.Parse(parameters[0]);
                bool isRow = parameters[1] == "r";
                if (!showTwoRow && isRow) //Return index for 1 row display
                {
                    return index;
                }
                else if (!showTwoRow && !isRow) //Return column 0 for one row display items
                {
                    return 0;
                }
                else if (showTwoRow && isRow) //Return index for 2 row display, after item 9th start from 0
                {
                    return index % MaxRow;
                }
                else if (showTwoRow && !isRow) //Return column 0 for 0-9th item | return 1 for 10th+ item
                {
                    return index < MaxRow ? 0 : 1;
                }
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
