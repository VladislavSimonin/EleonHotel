using EleonHotel.Windows.MainWindows.Admin;
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

namespace EleonHotel.Windows.MainWindows
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public int UserId { get; private set; }

        public AdminWindow(int userId)
        {
            InitializeComponent();
            UserId = userId;
        }

        private void fullUsersInfo_Click(object sender, RoutedEventArgs e)
        {
            new fullUsersInfo().ShowDialog();
        }

        private void roomsCondition_Click(object sender, RoutedEventArgs e)
        {
            new roomsCondition().ShowDialog();
        }

        private void deleteUser_Click(object sender, RoutedEventArgs e)
        {
            new deleteUser().ShowDialog();
        }

        private void roomsCost_Click(object sender, RoutedEventArgs e)
        {
            new roomsCost().ShowDialog();
        }

        private void staffInfo_Click(object sender, RoutedEventArgs e)
        {
            new staffInfo().ShowDialog();
        }

        private void Conflicts_Click(object sender, RoutedEventArgs e)
        {
            new Conflicts().ShowDialog();
        }

        private void guestsInfo_Click(object sender, RoutedEventArgs e)
        {
            new guestsInfo().ShowDialog();
        }
    }
}
