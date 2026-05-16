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

namespace EleonHotel.Windows.MainWindows.Staff.Cook
{
    /// <summary>
    /// Логика взаимодействия для cookWindow.xaml
    /// </summary>
    public partial class cookWindow : Window
    {
        private readonly int _userId;

        public cookWindow()
        {
            InitializeComponent();
        }

        public cookWindow(int userId) : this()
        {
            _userId = userId;
        }
    }
}
