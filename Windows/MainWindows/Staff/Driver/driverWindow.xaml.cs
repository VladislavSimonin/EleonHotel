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

namespace EleonHotel.Windows.MainWindows.Staff.Driver
{
    /// <summary>
    /// Логика взаимодействия для driverWindow.xaml
    /// </summary>
    public partial class driverWindow : Window
    {
        private readonly int _userId;

        public driverWindow()
        {
            InitializeComponent();
        }

        public driverWindow(int userId) : this()
        {
            _userId = userId;
        }
    }
}
