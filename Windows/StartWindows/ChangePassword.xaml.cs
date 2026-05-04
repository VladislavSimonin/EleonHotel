using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using EleonHotel.Data.Classes;
using Konscious.Security.Cryptography;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        private const string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        
        public ChangePassword()
        {
            InitializeComponent();
            DataContext = new RegistrationViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Получаем логин из настроек (сохраненный при восстановлении)
            string login = Properties.Settings.Default.RecoveryLogin;
            
            if (!string.IsNullOrEmpty(login))
            {
                TbLogin.Text = login;
            }
            else
            {
                MessageBox.Show("Ошибка: логин не найден. Пожалуйста, начните процесс восстановления заново.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Проверка: поле логина должно быть заполнено
            if (string.IsNullOrWhiteSpace(TbLogin.Text))
            {
                MessageBox.Show("Ошибка: логин не определен. Пожалуйста, начните процесс восстановления заново.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка паролей
            if (string.IsNullOrWhiteSpace(TbPassword.Text))
            {
                MessageBox.Show("Введите новый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PbConfirmPassword.Password))
            {
                MessageBox.Show("Подтвердите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PbConfirmPassword.Focus();
                return;
            }

            // Проверка совпадения паролей
            if (TbPassword.Text != PbConfirmPassword.Password)
            {
                MessageBox.Show("Пароли не совпадают. Пожалуйста, попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PbConfirmPassword.Clear();
                PbConfirmPassword.Focus();
                return;
            }

            // Проверка сложности пароля
            var viewModel = DataContext as RegistrationViewModel;
            if (viewModel == null || viewModel.StrengthPercent < 60) // Минимум "Средний" уровень
            {
                MessageBox.Show("Пароль слишком слабый. Пожалуйста, выберите более надежный пароль (минимум средний уровень надежности).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbPassword.Focus();
                return;
            }

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();

                string login = TbLogin.Text.Trim();
                string newPassword = TbPassword.Text.Trim();

                // Хэшируем новый пароль с помощью Argon2id
                var (hash, salt) = Argon2PasswordHasher.HashPassword(newPassword);

                // Обновляем пароль в базе данных
                using (var cmd = new SqlCommand(
                    "UPDATE Users SET password_hash = @hash, hash_salt = @salt WHERE login = @login",
                    conn))
                {
                    cmd.Parameters.Add("@hash", SqlDbType.VarBinary).Value = hash;
                    cmd.Parameters.Add("@salt", SqlDbType.VarBinary).Value = salt;
                    cmd.Parameters.AddWithValue("@login", login);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пароль успешно изменен! Теперь вы можете войти с новым паролем.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Очищаем сохраненный логин
                        Properties.Settings.Default.RecoveryLogin = null;
                        Properties.Settings.Default.Save();

                        // Открываем окно входа
                        new Welcome().Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении пароля. Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
