using EleonHotel.Data.Classes;
using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EleonHotel.Windows.StartWindows
{
    /// <summary>
    /// Логика взаимодействия для CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        bool captchaStatus = false;

        // Регулярные выражения для валидации
        private static readonly Regex CyrillicLettersRegex = new Regex(@"^[а-яА-ЯёЁa-zA-Z\s]+$");
        private static readonly Regex DigitsRegex = new Regex(@"^\d+$");
        private static readonly Regex PhoneRegex = new Regex(@"^\+7\d{10}$");
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly Regex LoginRegex = new Regex(@"^[a-zA-Z0-9_]{3,20}$");

        // Список для отслеживания полей с ошибками (для визуальной подсветки)
        private readonly List<Control> _errorControls = new List<Control>();

        public CreateAccount()
        {
            InitializeComponent();
            DataContext = new RegistrationViewModel();
            InitializeValidation();
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

            // Валидация при потере фокуса (теперь только проверяет и помечает, но не показывает MessageBox)
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
            e.Handled = !CyrillicLettersRegex.IsMatch(e.Text);
        }

        private void TbPassportSeries_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsRegex.IsMatch(e.Text);
        }

        private void TbPassportNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsRegex.IsMatch(e.Text);
        }

        private void TbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
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

        #region LostFocus Handlers (Теперь только визуальная проверка)

        private void TbSurname_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbSurname, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && CyrillicLettersRegex.IsMatch(text)));
        }

        private void TbName_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbName, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && CyrillicLettersRegex.IsMatch(text)));
        }

        private void TbPatronymic_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbPatronymic, !ValidateField(text => string.IsNullOrWhiteSpace(text) || CyrillicLettersRegex.IsMatch(text)));
        }

        private void TbPassportSeries_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbPassportSeries, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && text.Length == 4 && DigitsRegex.IsMatch(text)));
        }

        private void TbPassportNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbPassportNumber, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && text.Length == 6 && DigitsRegex.IsMatch(text)));
        }

        private void TbWhoGavePassport_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbWhoGavePassport, !ValidateField(text => !string.IsNullOrWhiteSpace(text)));
        }

        private void TbPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbPhone, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && PhoneRegex.IsMatch(text)));
        }

        private void TbEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbEmail, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && EmailRegex.IsMatch(text)));
        }

        private void TbLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            SetErrorBorder(TbLogin, !ValidateField(text => !string.IsNullOrWhiteSpace(text) && LoginRegex.IsMatch(text)));
        }

        #endregion

        #region DatePicker Handlers

        private void Birthsday_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Убираем визуальную ошибку при изменении
            SetErrorBorder(Birthsday, false);

            if (Birthsday.SelectedDate.HasValue)
            {
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;

                if (birthDate > today || age < 14)
                {
                    // Не показываем MessageBox здесь, просто помечаем как ошибку
                    SetErrorBorder(Birthsday, true);
                    Birthsday.SelectedDate = null; // Сбрасываем неверную дату
                }
            }
        }

        private void WhenPassportGave_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetErrorBorder(WhenPassportGave, false);

            if (WhenPassportGave.SelectedDate.HasValue && Birthsday.SelectedDate.HasValue)
            {
                var passportDate = WhenPassportGave.SelectedDate.Value;
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;

                if (passportDate > today || passportDate < birthDate.AddYears(14))
                {
                    SetErrorBorder(WhenPassportGave, true);
                    WhenPassportGave.SelectedDate = null;
                }
            }
        }

        #endregion

        // Вспомогательный метод для проверки одного поля
        private bool ValidateField(Func<string, bool> validationFunc)
        {
            // Этот метод используется в LostFocus, но логика вынесена в ValidateAllFields для кнопки
            return true;
        }

        // Установка красной рамки при ошибке
        private void SetErrorBorder(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BorderBrush = Brushes.Red;
                control.BorderThickness = new Thickness(2);
                if (!_errorControls.Contains(control))
                    _errorControls.Add(control);
            }
            else
            {
                control.ClearValue(BorderBrushProperty);
                control.ClearValue(BorderThicknessProperty);
                _errorControls.Remove(control);
            }
        }

        private bool ValidateAllFields()
        {
            var errors = new List<string>();
            _errorControls.Clear(); // Очищаем список перед новой проверкой

            // Сброс визуальных ошибок
            var allControls = new Control[] {
                TbSurname, TbName, TbPatronymic, TbPassportSeries, TbPassportNumber,
                TbWhoGavePassport, TbPhone, TbEmail, TbLogin, Birthsday, WhenPassportGave,
                TbRegistrationAddress, AnswerTextBox
            };
            foreach (var c in allControls) SetErrorBorder(c, false);

            // Валидация ФИО
            if (string.IsNullOrWhiteSpace(TbSurname.Text))
            {
                errors.Add("Фамилия не заполнена");
                SetErrorBorder(TbSurname, true);
            }
            else if (!CyrillicLettersRegex.IsMatch(TbSurname.Text.Trim()))
            {
                errors.Add("Фамилия должна содержать только буквы");
                SetErrorBorder(TbSurname, true);
            }

            if (string.IsNullOrWhiteSpace(TbName.Text))
            {
                errors.Add("Имя не заполнено");
                SetErrorBorder(TbName, true);
            }
            else if (!CyrillicLettersRegex.IsMatch(TbName.Text.Trim()))
            {
                errors.Add("Имя должно содержать только буквы");
                SetErrorBorder(TbName, true);
            }

            if (!string.IsNullOrWhiteSpace(TbPatronymic.Text) && !CyrillicLettersRegex.IsMatch(TbPatronymic.Text.Trim()))
            {
                errors.Add("Отчество должно содержать только буквы");
                SetErrorBorder(TbPatronymic, true);
            }

            // Валидация даты рождения
            if (!Birthsday.SelectedDate.HasValue)
            {
                errors.Add("Дата рождения не выбрана");
                SetErrorBorder(Birthsday, true);
            }
            else
            {
                var birthDate = Birthsday.SelectedDate.Value;
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;

                if (birthDate > today)
                {
                    errors.Add("Дата рождения не может быть в будущем");
                    SetErrorBorder(Birthsday, true);
                }
                else if (age < 14)
                {
                    errors.Add("Регистрация возможна только с 14 лет");
                    SetErrorBorder(Birthsday, true);
                }
            }

            // Валидация паспорта
            if (string.IsNullOrWhiteSpace(TbPassportSeries.Text))
            {
                errors.Add("Серия паспорта не заполнена");
                SetErrorBorder(TbPassportSeries, true);
            }
            else if (TbPassportSeries.Text.Length != 4 || !DigitsRegex.IsMatch(TbPassportSeries.Text))
            {
                errors.Add("Серия паспорта должна содержать ровно 4 цифры");
                SetErrorBorder(TbPassportSeries, true);
            }

            if (string.IsNullOrWhiteSpace(TbPassportNumber.Text))
            {
                errors.Add("Номер паспорта не заполнен");
                SetErrorBorder(TbPassportNumber, true);
            }
            else if (TbPassportNumber.Text.Length != 6 || !DigitsRegex.IsMatch(TbPassportNumber.Text))
            {
                errors.Add("Номер паспорта должен содержать ровно 6 цифр");
                SetErrorBorder(TbPassportNumber, true);
            }

            if (string.IsNullOrWhiteSpace(TbWhoGavePassport.Text))
            {
                errors.Add("Поле 'Кем выдан паспорт' не заполнено");
                SetErrorBorder(TbWhoGavePassport, true);
            }

            // Валидация даты выдачи паспорта
            if (!WhenPassportGave.SelectedDate.HasValue)
            {
                errors.Add("Дата выдачи паспорта не выбрана");
                SetErrorBorder(WhenPassportGave, true);
            }
            else if (WhenPassportGave.SelectedDate.Value > DateTime.Today)
            {
                errors.Add("Дата выдачи паспорта не может быть в будущем");
                SetErrorBorder(WhenPassportGave, true);
            }
            else if (Birthsday.SelectedDate.HasValue && WhenPassportGave.SelectedDate.Value < Birthsday.SelectedDate.Value.AddYears(14))
            {
                errors.Add("Дата выдачи паспорта не может быть раньше 14-летия");
                SetErrorBorder(WhenPassportGave, true);
            }

            // Валидация адреса
            if (string.IsNullOrWhiteSpace(TbRegistrationAddress.Text))
            {
                errors.Add("Адрес регистрации не заполнен");
                SetErrorBorder(TbRegistrationAddress, true);
            }

            // Валидация телефона
            if (string.IsNullOrWhiteSpace(TbPhone.Text))
            {
                errors.Add("Номер телефона не заполнен");
                SetErrorBorder(TbPhone, true);
            }
            else if (!PhoneRegex.IsMatch(TbPhone.Text.Trim()))
            {
                errors.Add("Номер телефона должен быть в формате +7XXXXXXXXXX");
                SetErrorBorder(TbPhone, true);
            }

            // Валидация email
            if (string.IsNullOrWhiteSpace(TbEmail.Text))
            {
                errors.Add("Электронная почта не заполнена");
                SetErrorBorder(TbEmail, true);
            }
            else if (!EmailRegex.IsMatch(TbEmail.Text.Trim()))
            {
                errors.Add("Некорректный формат электронной почты");
                SetErrorBorder(TbEmail, true);
            }

            // Валидация логина
            if (string.IsNullOrWhiteSpace(TbLogin.Text))
            {
                errors.Add("Логин не заполнен");
                SetErrorBorder(TbLogin, true);
            }
            else if (!LoginRegex.IsMatch(TbLogin.Text.Trim()))
            {
                errors.Add("Логин должен содержать от 3 до 20 символов (буквы, цифры, _)");
                SetErrorBorder(TbLogin, true);
            }

            else if (IsLoginExists(TbLogin.Text.Trim()))
                errors.Add("Такой логин уже существует. Пожалуйста, выберите другой.");

            // Валидация пароля
            if (string.IsNullOrWhiteSpace(TbPassword.Text))
            {
                errors.Add("Пароль не заполнен");
                // TbPassword тоже можно подсветить, если нужно
            }
            else
            {
                var viewModel = DataContext as RegistrationViewModel;
                if (viewModel != null && viewModel.StrengthPercent < 20)
                {
                    errors.Add("Пароль слишком слабый. Используйте заглавные буквы, цифры и спецсимволы");
                }
            }

            // Валидация роли
            if (RbGuest.IsChecked != true && RbEmployee.IsChecked != true)
            {
                errors.Add("Выберите кто Вы: Гость или Сотрудник");
            }

            // Валидация капчи
            if (string.IsNullOrWhiteSpace(AnswerTextBox.Text.Trim()))
            {
                errors.Add("Введите код с капчи");
                SetErrorBorder(AnswerTextBox, true);
            }
            else if (!captchaStatus)
            {
                errors.Add("Неправильный код капчи");
                SetErrorBorder(AnswerTextBox, true);
            }

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Устанавливаем фокус на первое поле с ошибкой
                if (_errorControls.Count > 0)
                {
                    _errorControls[0].Focus();
                }
                else if (!Birthsday.SelectedDate.HasValue && errors.Contains("Дата рождения не выбрана"))
                {
                    Birthsday.Focus();
                }

                return false;
            }

            return true;
        }

        private bool IsLoginExists(string login)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE login = @login";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке логина: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // В случае ошибки считаем, что логин существует, чтобы предотвратить регистрацию
                return true;
            }
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

                int newId;
                using (var cmdMax = new SqlCommand(
                    "select isnull(max(user_id), 0) from Users", conn, tran))
                {
                    newId = (int)cmdMax.ExecuteScalar() + 1;
                }

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

                // Добавляем роль
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
                BtnCreateAccount.IsEnabled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnswerTextBox.Text.ToLower() == MyCaptcha.CaptchaText.ToLower())
            {
                captchaStatus = true;
                SetErrorBorder(AnswerTextBox, false);
                MessageBox.Show("Верно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                captchaStatus = false;
                SetErrorBorder(AnswerTextBox, true);
                MessageBox.Show("Ошибка! Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
                AnswerTextBox.Clear();
                AnswerTextBox.Focus();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MyCaptcha.CreateCaptcha(EasyCaptcha.Wpf.Captcha.LetterOption.Alphanumeric, 5);
            AnswerTextBox.Clear();
            captchaStatus = false;
            SetErrorBorder(AnswerTextBox, false);
        }
    }
}