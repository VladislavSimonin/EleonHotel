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
using EleonHotel.Windows.MainWindows.Guest.AfterBooking;

namespace EleonHotel.Windows.MainWindows.Guest
{
    /// <summary>
    /// Логика взаимодействия для BookedGuestWindow.xaml
    /// </summary>
    public partial class BookedGuestWindow : Window
    {
        private readonly int _userId;

        public BookedGuestWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void aboutMe_Click(object sender, RoutedEventArgs e)
        {
            var aboutMeWindow = new aboutMe(_userId);
            aboutMeWindow.ShowDialog();
        }

        private void orderDish_Click(object sender, RoutedEventArgs e)
        {
            var dishWindow = new Dish(_userId);
            dishWindow.ShowDialog();
        }

        private void orderLaundry_Click(object sender, RoutedEventArgs e)
        {

        }

        private void orderCleaning_Click(object sender, RoutedEventArgs e)
        {

        }

        private void orderTransfer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void inviteDoctor_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
