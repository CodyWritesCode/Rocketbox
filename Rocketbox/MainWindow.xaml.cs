using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NHotkey;
using NHotkey.Wpf;

namespace Rocketbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            RbData.LoadData();

            this.Width = System.Windows.SystemParameters.WorkArea.Width - 40;
            textConsole.Width = this.Width - 20;
            responseText.Width = this.Width - 64;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = new Icon(@"icons/rocket.ico");
            _trayIcon.Visible = true;
            _trayIcon.Click += (sender, e) => { this.Show(); };
            _trayIcon.ShowBalloonTip(5000, "Rocketbox", "Rocketbox is now active. Press Win + ~ to open it.", System.Windows.Forms.ToolTipIcon.Info);

            HotkeyManager.Current.AddOrReplace("ShowRb", Key.OemTilde, ModifierKeys.Windows, OnHotkey);

            responseText.Text = string.Empty;
            textConsole.Text = string.Empty;

            Invoker.Invoke(textConsole.Text);

            // Event assignments:
            //  Window will close if unfocused
            //  Other window items will update if a key is pressed
            //  Text box will focus when focused (loaded)
            this.Deactivated += (sender, e) => this.Hide();
            this.KeyDown += KeyPress;
            this.Loaded += (sender, e) => textConsole.Focus();

            textConsole.TextChanged += TextUpdated;
        }

        private void TextUpdated(object sender, TextChangedEventArgs e)
        {
            // Invoker will process the command and return its result
            Invoker.Invoke(textConsole.Text);
            responseText.Text = Invoker.GetResponse();

            // Set an icon if the icon file listed in the command exists
            string icon = Invoker.GetIcon();
            if(icon != null && icon.Trim() != string.Empty)
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

        private void Reset()
        {
            textConsole.Clear();
            iconView.Source = null;
            responseText.Text = string.Empty;
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    Reset();
                    this.Hide();
                    break;
                case Key.Enter:
                    if(Invoker.Execute())
                    {
                        if(Invoker.ShutdownNow)
                        {
                            this.Close();
                        }
                        else
                        {
                            Reset();
                            this.Hide();
                        }
                    }
                    else
                    {
                        responseText.Text = Invoker.GetResponse();
                    }
                    break;
            }
        }

        private void OnHotkey(object sender, HotkeyEventArgs e)
        {
            this.Show();
        }
    }
}
