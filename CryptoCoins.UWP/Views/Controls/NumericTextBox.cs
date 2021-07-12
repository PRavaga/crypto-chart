using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CryptoCoins.UWP.Views.Controls
{
    public class NumericTextBox : TextBox
    {
        private bool _updateText = true;
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(decimal?), typeof(NumericTextBox), new PropertyMetadata(default(decimal?), PropertyChangedCallback));

        public NumericTextBox()
        {
            //DefaultStyleKey = typeof(NumericTextBox);
            TextChanging += NumericTextBox_TextChanging;
            TextChanged += NumericTextBox_TextChanged;
        }

        public int MaxPrecision { get; set; } = 15;

        public decimal? Value
        {
            get => (decimal?) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (NumericTextBox) dependencyObject;
            if (!textBox._updateText)
            {
                return;
            }
            var newValue = (decimal?) e.NewValue;
            if (newValue.HasValue)
            {
                var str = newValue.Value.ToString("F15", CultureInfo.InvariantCulture);
                if (str.Contains('.')|| str.Contains(','))
                {
                    str = str.TrimEnd('0').TrimEnd('.',',');
                }
                textBox.Text = str;
            }
            else
            {
                textBox.Text = String.Empty;
            }
        }

        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox) sender;
            var separatorCount = 0;
            var justNumbers = new string(box.Text.Where(c => char.IsDigit(c) || c == '.' && separatorCount++ == 0 || c == ',' && separatorCount++ == 0).Take(MaxPrecision).ToArray());
            var lengthToEnd = box.Text.Length - (box.SelectionStart + SelectionLength);
            box.Text = justNumbers;
            var selectionStart = box.Text.Length - lengthToEnd;
            box.SelectionStart = selectionStart > 0 ? selectionStart : 0;
            _updateText = false;
            if (decimal.TryParse(justNumbers.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
            {
                Value = parsedValue;
            }
            else
            {
                Value = null;
            }
            _updateText = true;
        }

        private void NumericTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
        }
    }
}
