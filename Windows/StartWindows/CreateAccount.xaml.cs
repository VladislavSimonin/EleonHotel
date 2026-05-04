using EleonHotel.Data.Classes;
using Konscious.Security.Cryptography;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
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
        private string ConnectionString = Properties.Settings.Default.ConnectionString;
        bool captchaStatus = false;
        public CreateAccount()
        {
            InitializeComponent();
            DataContext = new RegistrationViewModel();
        }

        private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(TbPassword.Text) ||
                string.IsNullOrWhiteSpace(TbSurname.Text) || string.IsNullOrWhiteSpace(TbName.Text) ||
                string.IsNullOrWhiteSpace(TbPatronymic.Text) || Birthsday.SelectedDate == null || string.IsNullOrWhiteSpace(TbPhone.Text) ||
                string.IsNullOrWhiteSpace(TbEmail.Text) || string.IsNullOrWhiteSpace(TbPassportSeries.Text) ||
                string.IsNullOrWhiteSpace(TbPassportNumber.Text) || string.IsNullOrWhiteSpace(TbWhoGavePassport.Text) ||
                WhenPassportGave.SelectedDate == null || string.IsNullOrWhiteSpace(TbRegistrationAddress.Text) ||
                RbGuest.IsChecked == false && RbEmployee.IsChecked == false || string.IsNullOrWhiteSpace(AnswerTextBox.Text.Trim()) ||
                captchaStatus == false)
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var tran = conn.BeginTransaction();
            try
            {
                BtnCreateAccount.IsEnabled = false;
                // 1. Получаем MAX(user_id)
                using (var cmdMax = new SqlCommand(
                    "select isnull(max(user_id), 0) from Users", conn, tran))
                {
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

                    // INSERT в Users
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Users(user_id, password_hash, hash_salt, login, " +
                        "surname, name, patronymic, birthsday, passport_series, passport_number, " +
                        "who_gave_passport, when_gave_passport, registration_address, phone, email) " +
                        "VALUES(@id, @hash, @salt, @login, @surname, @name, @patronymic, @birthsday, " +
                        "@series, @number, @whoGave, @whenGave, @address, @phone, @email)",
                        conn, tran))
                    {
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
                    }

                    // Добавляем роль (Guest или Employee) в ту же транзакцию
                    if (RbGuest.IsChecked == true)
                    {
                        using (var cmdMaxGuest = new SqlCommand(
                            "select isnull(max(guest_id), 0) from Guests", conn, tran))
                        {
                            int newGuestId = (int)cmdMaxGuest.ExecuteScalar() + 1;

                            using (var cmdGuest = new SqlCommand(
                                "insert into Guests(guest_id, user_id, room_id) values(@guest_id, @id, NULL)", conn, tran))
                            {
                                cmdGuest.Parameters.AddWithValue("@guest_id", newGuestId);
                                cmdGuest.Parameters.AddWithValue("@id", newId);

                                cmdGuest.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (RbEmployee.IsChecked == true)
                    {
                        using (var cmdMaxEmployee = new SqlCommand(
                            "select isnull(max(employee_id), 0) from Employees", conn, tran))
                        {
                            int newEmployeeId = (int)cmdMaxEmployee.ExecuteScalar() + 1;

                            using (var cmdEmployee = new SqlCommand(
                                "insert into Employees(employee_id, user_id, shift_id, salary, position_id) " +
                                "values (@employee_id, @id, NULL, NULL, NULL)", conn, tran))
                            {
                                cmdEmployee.Parameters.AddWithValue("@employee_id", newEmployeeId);
                                cmdEmployee.Parameters.AddWithValue("@id", newId);

                                cmdEmployee.ExecuteNonQuery();
                            }
                        }
                    }
                }

                tran.Commit();

                MessageBox.Show("Вы успешно зарегистрированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                new Welcome().Show();
                this.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    tran.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine("Ошибка при откате: " + rollbackEx.Message);
                }
                MessageBox.Show("Ошибка при регистрации: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Генерируем капчу при загрузке (5 символов, буквы и цифры)
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Сравниваем текст из TextBox с текстом на капче
            if (AnswerTextBox.Text.ToLower() == MyCaptcha.CaptchaText.ToLower())
            {
                captchaStatus = true;
                MessageBox.Show("Верно!");
            }
            else
            {
                captchaStatus = false;
                MessageBox.Show("Ошибка! Попробуйте снова.");
                // Обновляем капчу при ошибке
                MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
        }
    }
}
