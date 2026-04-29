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
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using EleonHotel.Data;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        public CreateAccount()
        {
            InitializeComponent();
        }

        private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            HotelDbContext dbContext = new HotelDbContext();

            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(TbPassword.Text) ||
            string.IsNullOrWhiteSpace(TbSurname.Text) || string.IsNullOrWhiteSpace(TbName.Text) ||
            string.IsNullOrWhiteSpace(TbPatronymic.Text) || string.IsNullOrWhiteSpace(TbPhone.Text) ||
            string.IsNullOrWhiteSpace(TbEmail.Text) || string.IsNullOrWhiteSpace(TbPassportSeries.Text) ||
            string.IsNullOrWhiteSpace(TbPassportNumber.Text) || string.IsNullOrWhiteSpace(TbWhoGavePassport.Text) ||
            string.IsNullOrWhiteSpace(TbWhenPassportGave.Text) || string.IsNullOrWhiteSpace(TbRegistrationAddress.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

                



   
    }
}
