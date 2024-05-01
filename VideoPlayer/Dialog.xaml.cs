using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        private string _value;
        public string Value => _value;

        private readonly Regex? _regex;

        public Dialog(string title = "Dialog", string? label = null, Regex? regex = null)
        {
            InitializeComponent();
            DialogWindow.Title = title;
            Label.Content = label;
            _regex = regex;

        }

        private void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            _value = TextBox.Text;
            DialogResult = true;
            Close();
        }

        private void TextBox_Validation(object sender, TextCompositionEventArgs e)
        {
            if (_regex == null) return;

            Regex regex = _regex;
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
