using System;
using System.Linq;
using System.Windows;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.RegularExpressions;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для AccountRecovery.xaml
    /// </summary>
    public partial class AccountRecovery : Window
    {
        // Переменная для хранения сгенерированного кода в текущей сессии
        private string _generatedCode;

        // Флаг, подтверждающий успешную проверку капчи и отправку письма
        private bool _isCaptchaPassed = false;

        public AccountRecovery()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);

            // Блокируем поле кода до прохождения капчи
            TbCode.IsEnabled = false;
            BtnAccept.IsEnabled = false;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Сравниваем текст из TextBox с текстом на капче
            if (string.IsNullOrWhiteSpace(AnswerTextBox.Text))
            {
                MessageBox.Show("Введите текст с картинки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AnswerTextBox.Text.Trim().ToLower() == MyCaptcha.CaptchaText.ToLower())
            {
                _isCaptchaPassed = true;
                MessageBox.Show("Капча пройдена!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                // Разблокируем ввод логина и email для отправки кода
                TbLogin.IsEnabled = true;
                TbEmail.IsEnabled = true;
                TbLogin.Focus();
                BtnAccept.IsEnabled = true;

                MessageBox.Show("Введите ваш логин и электронную почту, затем нажмите кнопку отправки кода.", "Инструкция", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ошибка! Попробуйте снова.");

                // Обновляем капчу при ошибке
                MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
                AnswerTextBox.Clear();
                AnswerTextBox.Focus();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Ручное обновление капчи
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
            AnswerTextBox.Clear();
            AnswerTextBox.Focus();
        }

        // Метод для отправки кода 

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            // Проверка капчи перед любыми действиями
            if (!_isCaptchaPassed)
            {
                MessageBox.Show("Сначала подтвердите, что вы не робот.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация полей
            if (string.IsNullOrWhiteSpace(TbLogin.Text.Trim()))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbLogin.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TbEmail.Text.Trim()))
            {
                MessageBox.Show("Введите Email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbEmail.Focus();
                return;
            }

            // Валидация формата Email
            if (!IsValidEmail(TbEmail.Text.Trim()))
            {
                MessageBox.Show("Некорректный формат Email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbEmail.Focus();
                return;
            }

            // Если код еще не был отправлен пользователю
            if (string.IsNullOrEmpty(_generatedCode))
            {
                SendVerificationCode();
            }
            else
            {
                // Если код уже отправлен, проверяем введенное значение
                VerifyCode();
            }
        }

        private void SendVerificationCode()
        {
            try
            {
                // 1. Генерация кода (6 цифр)
                Random random = new Random();
                _generatedCode = random.Next(100000, 999999).ToString();

                // 2. НАСТРОЙКИ SMTP 
                string smtpServer = Properties.Settings.Default.smtpServer; 
                int smtpPort = Properties.Settings.Default.smtpPort;
                string senderEmail = Properties.Settings.Default.senderEmail;
                string senderPassword = Properties.Settings.Default.senderPassword;
                string senderName = Properties.Settings.Default.senderName;

                // 3. Создание письма
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", TbEmail.Text.Trim()));
                message.Subject = "Код восстановления доступа - Отель Элеон";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = $"Здравствуйте!\n\nВаш код подтверждения для восстановления доступа: {_generatedCode}\n\nВведите этот код в поле 'Код подтверждения' в приложении.\n\nЕсли вы не запрашивали восстановление, просто проигнорируйте это письмо.";
                message.Body = bodyBuilder.ToMessageBody();

                // 4. Отправка
                using (var client = new SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                    // Аутентификация
                    client.Authenticate(senderEmail, senderPassword);

                    client.Send(message);
                    client.Disconnect(true);
                }

                MessageBox.Show($"Код успешно отправлен на {TbEmail.Text}!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                // Активируем поле ввода кода
                TbCode.IsEnabled = true;
                TbCode.Focus();

                BtnAccept.Content = "Подтвердить код";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке письма: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _generatedCode = null; // Сбрасываем код при ошибке
            }
        }

        private void VerifyCode()
        {
            if (string.IsNullOrWhiteSpace(TbCode.Text.Trim()))
            {
                MessageBox.Show("Введите код из письма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCode.Focus();
                return;
            }

            if (TbCode.Text.Trim() == _generatedCode)
            {
                MessageBox.Show("Код верен! Теперь вы можете ввести новый пароль.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                new ChangePassword().ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код подтверждения. Попробуйте снова или запросите новый код.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCode.Clear();
                TbCode.Focus();
            }
        }

        // Вспомогательный метод для валидации Email
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Простая проверка формата через регулярное выражение
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}