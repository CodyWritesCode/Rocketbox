using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rocketbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RbData.LoadData();

            responseText.Text = string.Empty;
            textConsole.Text = string.Empty;

            Invoker.Invoke(textConsole.Text);

            //this.Deactivated += (sender, e) => this.Close();
            this.KeyDown += KeyPress;
            this.Loaded += (sender, e) => textConsole.Focus();

            textConsole.TextChanged += TextUpdated;
        }

        private void TextUpdated(object sender, TextChangedEventArgs e)
        {
            Invoker.Invoke(textConsole.Text);
            responseText.Text = Invoker.GetResponse();

            string icon = Invoker.GetIcon();
            if(icon.Trim() != string.Empty)
            {
                if(RbUtility.IconExists(icon))
                {
                    iconView.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\icons\\" + icon));
                }
            }
            else
            {
                iconView.Source = null;
            }
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
                case Key.Enter:
                    if(Invoker.Execute())
                    {
                        this.Close();
                    }
                    break;
            }
        }
    }
}
