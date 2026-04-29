using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth * 0.5; // 80% ширины
            this.Height = SystemParameters.PrimaryScreenHeight * 0.5; // 80% высоты
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            new CreateAccount().Show();
            this.Close();
        }
    }
}
