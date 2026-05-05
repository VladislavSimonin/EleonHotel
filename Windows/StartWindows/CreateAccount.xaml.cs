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
using EleonHotel.Helpers;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        bool captchaStatus = false;
        
        // Помощники DaData для автодополнения
        private DadataTextBoxHelper _addressHelper;
        private DadataTextBoxHelper _surnameHelper;
        private DadataTextBoxHelper _nameHelper;
        private DadataTextBoxHelper _patronymicHelper;
        private DadataTextBoxHelper _phoneHelper;
        private DadataTextBoxHelper _emailHelper;
        
        // Регулярные выражения для валидации
        private static readonly System.Text.RegularExpressions.Regex CyrillicLettersRegex = 
            new System.Text.RegularExpressions.Regex(@"^[а-яА-ЯёЁa-zA-Z\s]+$");
        private static readonly System.Text.RegularExpressions.Regex DigitsRegex = 
            new System.Text.RegularExpressions.Regex(@"^\d+$");
        private static readonly System.Text.RegularExpressions.Regex PhoneRegex = 
            new System.Text.RegularExpressions.Regex(@"^\+7\d{10}$");
        private static readonly System.Text.RegularExpressions.Regex EmailRegex = 
            new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly System.Text.RegularExpressions.Regex LoginRegex = 
            new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9_]{3,20}$");
        
        public CreateAccount()
        {
            InitializeComponent();
            DataContext = new RegistrationViewModel();
            InitializeValidation();
            InitializeDadataAutoComplete();
        }

        /// <summary>
        /// Инициализация автодополнения DaData для TextBox
        /// </summary>
        private void InitializeDadataAutoComplete()
        {
            // Подключаем автодополнение для адреса регистрации
            _addressHelper = new DadataTextBoxHelper(TbRegistrationAddress, DadataTextBoxHelper.PopupType.Address);
            
            // Подключаем автодополнение для ФИО (фамилия, имя, отчество)
            _surnameHelper = new DadataTextBoxHelper(TbSurname, DadataTextBoxHelper.PopupType.Fio);
            _nameHelper = new DadataTextBoxHelper(TbName, DadataTextBoxHelper.PopupType.Fio);
            _patronymicHelper = new DadataTextBoxHelper(TbPatronymic, DadataTextBoxHelper.PopupType.Fio);
            
            // Подключаем автодополнение для телефона
            _phoneHelper = new DadataTextBoxHelper(TbPhone, DadataTextBoxHelper.PopupType.Phone);
            
            // Подключаем автодополнение для email
            _emailHelper = new DadataTextBoxHelper(TbEmail, DadataTextBoxHelper.PopupType.Email);
        }

        private void InitializeValidation()
        {
            // Валидация ФИО - только буквы и пробелы
            TbSurname.PreviewTextInput += TbName_PreviewTextInput;
            TbName.PreviewTextInput += TbName_PreviewTextInput;
            TbPatronymic.PreviewTextInput += TbName_PreviewTextInput;
            
            // Валидация серии и номера паспорта - только цифры
            TbPassportSeries.PreviewTextInput += TbPassportSeries_PreviewTextInput;
            TbPassportNumber.PreviewTextInput += TbPassportNumber_PreviewTextInput;
            
            // Валидация телефона - только цифры и +
            TbPhone.PreviewTextInput += TbPhone_PreviewTextInput;
            
            // Валидация при потере фокуса
            TbSurname.LostFocus += TbSurname_LostFocus;
            TbName.LostFocus += TbName_LostFocus;
            TbPatronymic.LostFocus += TbPatronymic_LostFocus;
            TbPassportSeries.LostFocus += TbPassportSeries_LostFocus;
            TbPassportNumber.LostFocus += TbPassportNumber_LostFocus;
            TbWhoGavePassport.LostFocus += TbWhoGavePassport_LostFocus;
            TbPhone.LostFocus += TbPhone_LostFocus;
            TbEmail.LostFocus += TbEmail_LostFocus;
            TbLogin.LostFocus += TbLogin_LostFocus;
            Birthsday.SelectedDateChanged += Birthsday_SelectedDateChanged;
            WhenPassportGave.SelectedDateChanged += WhenPassportGave_SelectedDateChanged;
        }

        #region PreviewTextInput Handlers
        
        private void TbName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только буквы и пробелы
            e.Handled = !CyrillicLettersRegex.IsMatch(e.Text);
        }

        private void TbPassportSeries_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !DigitsRegex.IsMatch(e.Text);
        }

        private void TbPassportNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !DigitsRegex.IsMatch(e.Text);
        }

        private void TbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и + (в начале)
            var textBox = sender as TextBox;
            if (e.Text == "+" && textBox.Text.Length == 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = !DigitsRegex.IsMatch(e.Text);
            }
        }
        
        #endregion

        #region LostFocus Handlers
        
        private void TbSurname_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbSurname, "Фамилия должна содержать только буквы", 
                text => !string.IsNullOrWhiteSpace(text) && CyrillicLettersRegex.IsMatch(text));
        }

        private void TbName_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbName, "Имя должно содержать только буквы", 
                text => !string.IsNullOrWhiteSpace(text) && CyrillicLettersRegex.IsMatch(text));
        }

        private void TbPatronymic_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbPatronymic, "Отчество должно содержать только буквы", 
                text => string.IsNullOrWhiteSpace(text) || CyrillicLettersRegex.IsMatch(text));
        }

        private void TbPassportSeries_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbPassportSeries, "Серия паспорта должна содержать ровно 4 цифры", 
                text => !string.IsNullOrWhiteSpace(text) && text.Length == 4 && DigitsRegex.IsMatch(text));
        }

        private void TbPassportNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbPassportNumber, "Номер паспорта должен содержать ровно 6 цифр", 
                text => !string.IsNullOrWhiteSpace(text) && text.Length == 6 && DigitsRegex.IsMatch(text));
        }

        private void TbWhoGavePassport_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbWhoGavePassport, "Поле 'Кем выдан паспорт' не может быть пустым", 
                text => !string.IsNullOrWhiteSpace(text));
        }

        private void TbPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbPhone, "Номер телефона должен быть в формате +7XXXXXXXXXX", 
                text => !string.IsNullOrWhiteSpace(text) && PhoneRegex.IsMatch(text));
        }

        private void TbEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbEmail, "Некорректный формат электронной почты", 
                text => !string.IsNullOrWhiteSpace(text) && EmailRegex.IsMatch(text));
        }

        private void TbLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateTextBox(TbLogin, "Логин должен содержать от 3 до 20 символов (буквы, цифры, _)", 
                text => !string.IsNullOrWhiteSpace(text) && LoginRegex.IsMatch(text));
        }
        
        #endregion

        #region DatePicker Handlers
        
        private void Birthsday_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Birthsday.SelectedDate.HasValue)
            {
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                
                if (birthDate > today)
                {
                    MessageBox.Show("Дата рождения не может быть в будущем!", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    Birthsday.SelectedDate = null;
                    return;
                }
                
                if (age < 14)
                {
                    MessageBox.Show("Регистрация возможна только с 14 лет!", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    Birthsday.SelectedDate = null;
                    return;
                }
            }
        }

        private void WhenPassportGave_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhenPassportGave.SelectedDate.HasValue && Birthsday.SelectedDate.HasValue)
            {
                var passportDate = WhenPassportGave.SelectedDate.Value;
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;
                
                if (passportDate > today)
                {
                    MessageBox.Show("Дата выдачи паспорта не может быть в будущем!", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    WhenPassportGave.SelectedDate = null;
                    return;
                }
                
                if (passportDate < birthDate.AddYears(14))
                {
                    MessageBox.Show("Дата выдачи паспорта не может быть раньше 14-летия!", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    WhenPassportGave.SelectedDate = null;
                    return;
                }
            }
        }
        
        #endregion

        private void ValidateTextBox(TextBox textBox, string errorMessage, Func<string, bool> validationFunc)
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text) && !validationFunc(textBox.Text.Trim()))
            {
                MessageBox.Show(errorMessage, "Ошибка валидации", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private bool ValidateAllFields()
        {
            var errors = new List<string>();

            // Валидация ФИО
            if (string.IsNullOrWhiteSpace(TbSurname.Text))
                errors.Add("Фамилия не заполнена");
            else if (!CyrillicLettersRegex.IsMatch(TbSurname.Text.Trim()))
                errors.Add("Фамилия должна содержать только буквы");

            if (string.IsNullOrWhiteSpace(TbName.Text))
                errors.Add("Имя не заполнено");
            else if (!CyrillicLettersRegex.IsMatch(TbName.Text.Trim()))
                errors.Add("Имя должно содержать только буквы");

            if (!string.IsNullOrWhiteSpace(TbPatronymic.Text) && !CyrillicLettersRegex.IsMatch(TbPatronymic.Text.Trim()))
                errors.Add("Отчество должно содержать только буквы");

            // Валидация даты рождения
            if (!Birthsday.SelectedDate.HasValue)
                errors.Add("Дата рождения не выбрана");
            else
            {
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                
                if (birthDate > today)
                    errors.Add("Дата рождения не может быть в будущем");
                else if (age < 14)
                    errors.Add("Регистрация возможна только с 14 лет");
            }

            // Валидация паспорта
            if (string.IsNullOrWhiteSpace(TbPassportSeries.Text))
                errors.Add("Серия паспорта не заполнена");
            else if (TbPassportSeries.Text.Length != 4 || !DigitsRegex.IsMatch(TbPassportSeries.Text))
                errors.Add("Серия паспорта должна содержать ровно 4 цифры");

            if (string.IsNullOrWhiteSpace(TbPassportNumber.Text))
                errors.Add("Номер паспорта не заполнен");
            else if (TbPassportNumber.Text.Length != 6 || !DigitsRegex.IsMatch(TbPassportNumber.Text))
                errors.Add("Номер паспорта должен содержать ровно 6 цифр");

            if (string.IsNullOrWhiteSpace(TbWhoGavePassport.Text))
                errors.Add("Поле 'Кем выдан паспорт' не заполнено");

            // Валидация даты выдачи паспорта
            if (!WhenPassportGave.SelectedDate.HasValue)
                errors.Add("Дата выдачи паспорта не выбрана");
            else if (WhenPassportGave.SelectedDate.Value > DateTime.Today)
                errors.Add("Дата выдачи паспорта не может быть в будущем");
            else if (Birthsday.SelectedDate.HasValue && WhenPassportGave.SelectedDate.Value < Birthsday.SelectedDate.Value.AddYears(14))
                errors.Add("Дата выдачи паспорта не может быть раньше 14-летия");

            // Валидация адреса
            if (string.IsNullOrWhiteSpace(TbRegistrationAddress.Text))
                errors.Add("Адрес регистрации не заполнен");

            // Валидация телефона
            if (string.IsNullOrWhiteSpace(TbPhone.Text))
                errors.Add("Номер телефона не заполнен");
            else if (!PhoneRegex.IsMatch(TbPhone.Text.Trim()))
                errors.Add("Номер телефона должен быть в формате +7XXXXXXXXXX");

            // Валидация email
            if (string.IsNullOrWhiteSpace(TbEmail.Text))
                errors.Add("Электронная почта не заполнена");
            else if (!EmailRegex.IsMatch(TbEmail.Text.Trim()))
                errors.Add("Некорректный формат электронной почты");

            // Валидация логина
            if (string.IsNullOrWhiteSpace(TbLogin.Text))
                errors.Add("Логин не заполнен");
            else if (!LoginRegex.IsMatch(TbLogin.Text.Trim()))
                errors.Add("Логин должен содержать от 3 до 20 символов (буквы, цифры, _)");

            // Валидация пароля (проверка через ViewModel)
            if (string.IsNullOrWhiteSpace(TbPassword.Text))
                errors.Add("Пароль не заполнен");
            else
            {
                var viewModel = DataContext as RegistrationViewModel;
                if (viewModel != null && viewModel.StrengthPercent < 20)
                    errors.Add("Пароль слишком слабый. Используйте заглавные буквы, цифры и спецсимволы");
            }

            // Валидация роли
            if (RbGuest.IsChecked != true && RbEmployee.IsChecked != true)
                errors.Add("Выберите кто Вы: Гость или Сотрудник");

            // Валидация капчи
            if (string.IsNullOrWhiteSpace(AnswerTextBox.Text.Trim()))
                errors.Add("Введите код с капчи");
            else if (!captchaStatus)
                errors.Add("Неправильный код капчи");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибка валидации", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            // Комплексная валидация всех полей перед отправкой
            if (!ValidateAllFields())
            {
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

        /// <summary>
        /// Очистка ресурсов DaData при закрытии окна
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Освобождаем ресурсы помощников DaData
            _addressHelper?.Cleanup();
            _surnameHelper?.Cleanup();
            _nameHelper?.Cleanup();
            _patronymicHelper?.Cleanup();
            _phoneHelper?.Cleanup();
            _emailHelper?.Cleanup();
            
            base.OnClosed(e);
        }
    }
}
