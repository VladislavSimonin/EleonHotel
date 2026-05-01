using EleonHotel.Data;
using Konscious.Security.Cryptography;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Data.Entity.Infrastructure.Design.Executor;
using static System.Net.Mime.MediaTypeNames;

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

            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(TbPassword.Text) ||
            string.IsNullOrWhiteSpace(TbSurname.Text) || string.IsNullOrWhiteSpace(TbName.Text) ||
            string.IsNullOrWhiteSpace(TbPatronymic.Text) || Birthsday.SelectedDate == null || string.IsNullOrWhiteSpace(TbPhone.Text) ||
            string.IsNullOrWhiteSpace(TbEmail.Text) || string.IsNullOrWhiteSpace(TbPassportSeries.Text) ||
            string.IsNullOrWhiteSpace(TbPassportNumber.Text) || string.IsNullOrWhiteSpace(TbWhoGavePassport.Text) ||
            WhenPassportGave.SelectedDate == null || string.IsNullOrWhiteSpace(TbRegistrationAddress.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            

            else
            {
                using var conn = new SqlConnection("Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;");
                conn.Open();

                using var tran = conn.BeginTransaction();
                try
                {
                    // 1. Получаем MAX(user_id). isNull гарантирует 0, если таблица пустая
                    using var cmdMax = new SqlCommand(
                        "select isnull(max(user_id), 0) from Users", conn, tran);


                    int newId = (int)cmdMax.ExecuteScalar() + 1;
                    var password = TbPassword.Text.Trim();
                    var login = TbLogin.Text.Trim();
                    var surname = TbSurname.Text.Trim();
                    var name = TbName.Text.Trim();
                    var patronymic = TbPatronymic.Text.Trim();
                    var birthsday = Birthsday.SelectedDate.Value;
                    var phone = TbPhone.Text.Trim();
                    var email = TbEmail.Text.Trim();
                    var series = TbPassportSeries.Text.Trim();
                    var number = TbPassportNumber.Text.Trim();
                    var whoGave = TbWhoGavePassport.Text.Trim();
                    var whenGave = WhenPassportGave.SelectedDate.Value;
                    var address = TbRegistrationAddress.Text.Trim();

                    // Хэшируем пароль
                    var (hash, salt) = Argon2PasswordHasher.HashPassword(password);

                    using var cmd = new SqlCommand(
                        "INSERT INTO Users(user_id, password_hash, hash_salt, login, " +
                        "surname, name, patronymic, birthsday, passport_series, passport_number, " +
                        "who_gave_passport, when_gave_passport, registration_address, phone, email) " +
                        "VALUES(@id, @hash, @salt, @login, @surname, @name, @patronymic, @birthsday, " +
                        "@series, @number, @whoGave, @whenGave, @address, @phone, @email)",
                        conn, tran);

                    cmd.Parameters.AddWithValue("@id", newId);
                    cmd.Parameters.Add("@hash", SqlDbType.VarBinary).Value = hash;
                    cmd.Parameters.Add("@salt", SqlDbType.VarBinary).Value = salt;
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@surname", surname);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@patronymic", patronymic);
                    cmd.Parameters.AddWithValue("@birthsday", birthsday);
                    cmd.Parameters.AddWithValue("@series", series);
                    cmd.Parameters.AddWithValue("@number", number);
                    cmd.Parameters.AddWithValue("@whoGave", whoGave);
                    cmd.Parameters.AddWithValue("@whenGave", whenGave);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);


                    cmd.ExecuteNonQuery();
                    tran.Commit();

                    MessageBox.Show("Успех");
                    return;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    tran.Rollback();
                    throw;
                }




                
            }

            
        }

        
    }
}
