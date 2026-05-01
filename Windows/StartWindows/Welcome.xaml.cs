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
using EleonHotel.Data.Classes;
using System.Data;
using System.Data.SqlClient;
using EleonHotel.Windows.MainWindows;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        private const string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";

        public Welcome()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth * 0.5; // 50% ширины
            this.Height = SystemParameters.PrimaryScreenHeight * 0.5; // 50% высоты
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            new CreateAccount().Show();
            this.Close();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Получаем хэш и соль пароля по логину
                    using (var cmd = new SqlCommand(
                        "SELECT user_id, password_hash, hash_salt FROM Users WHERE login = @login",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = (int)reader["user_id"];
                                byte[] storedHash = (byte[])reader["password_hash"];
                                byte[] storedSalt = (byte[])reader["hash_salt"];

                                // Верифицируем пароль
                                if (Argon2PasswordHasher.VerifyPassword(password, storedHash, storedSalt))
                                {
                                    // Успешная авторизация
                                    MessageBox.Show("Добро пожаловать!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    NavigateUser(userId);
                                }
                                else
                                {
                                    // Неверный пароль
                                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    PbPassword.Clear();
                                }
                            }
                        }           
                    }
                }
                

            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateUser(int userId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Проверяем, есть ли пользователь в таблице Guests
                    using (var cmd = new SqlCommand(
                        "SELECT guest_id FROM Guests WHERE user_id = @userId",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Пользователь является гостем
                                new GuestWindow().Show();
                                this.Close();
                                return;
                            }
                        }
                    }

                    // Проверяем, есть ли пользователь в таблице Employees с employee_id = 1 или 2
                    using (var cmd = new SqlCommand(
                        "SELECT employee_id FROM Employees WHERE user_id = @userId AND employee_id IN (1, 2)",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Пользователь является администратором
                                new AdminWindow().Show();
                                this.Close();
                                return;
                            }
                        }
                    }

                    // Если пользователь не найден ни в одной из таблиц
                    MessageBox.Show("Ошибка: Пользователь не найден в системе. Обратитесь к администратору.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при навигации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
