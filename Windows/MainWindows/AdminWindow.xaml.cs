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
            _context = new HotelDbContext();
        }

        private void BtnFullUsersInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var users = _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.Surname,
                        u.Name,
                        u.Patronymic,
                        u.PassportSeries,
                        u.PassportNumber,
                        u.WhoGavePassport,
                        u.WhenGavePassport,
                        u.RegistrationAddress,
                        u.Phone,
                        u.Email,
                        u.Birthsday
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Информация о пользователях",
                    Width = 1200,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = users
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("UserId"), Width = 50 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("Surname") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("Name") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("Patronymic") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Серия паспорта", Binding = new System.Windows.Data.Binding("PassportSeries"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Номер паспорта", Binding = new System.Windows.Data.Binding("PassportNumber"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Кем выдан", Binding = new System.Windows.Data.Binding("WhoGavePassport"), Width = 150 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата выдачи", Binding = new System.Windows.Data.Binding("WhenGavePassport"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Адрес регистрации", Binding = new System.Windows.Data.Binding("RegistrationAddress"), Width = 150 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email"), Width = 150 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата рождения", Binding = new System.Windows.Data.Binding("Birthsday"), Width = 100 });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRoomsCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rooms = _context.Rooms
                    .Include(r => r.RoomCategory)
                    .Include(r => r.RoomStatus)
                    .Include(r => r.Guests)
                    .Select(r => new
                    {
                        r.RoomId,
                        CategoryName = r.RoomCategory.RoomCategoryName,
                        Status = r.RoomStatus.RoomStatus1,
                        GuestsCount = r.Guests.Count,
                        Cost = r.RoomCategory.Cost
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Состояние номеров",
                    Width = 900,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = rooms
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID номера", Binding = new System.Windows.Data.Binding("RoomId"), Width = 80 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new System.Windows.Data.Binding("CategoryName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new System.Windows.Data.Binding("Status") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Гостей", Binding = new System.Windows.Data.Binding("GuestsCount"), Width = 80 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Стоимость", Binding = new System.Windows.Data.Binding("Cost", StringFormat = "{0:N0} ₽"), Width = 100 });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var users = _context.Users
                    .Select(u => new { u.UserId, u.Surname, u.Name, u.Patronymic, u.Login })
                    .ToList();

                var window = new Window
                {
                    Title = "Удаление пользователей",
                    Width = 700,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = users
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("UserId"), Width = 50 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("Surname") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("Name") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("Patronymic") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Логин", Binding = new System.Windows.Data.Binding("Login") });

                var deleteButton = new Button
                {
                    Content = "Удалить выбранного пользователя",
                    Margin = new Thickness(10),
                    Padding = new Thickness(15, 10),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                deleteButton.Click += (s, args) =>
                {
                    if (dataGrid.SelectedItem == null)
                    {
                        MessageBox.Show("Выберите пользователя для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var selectedUser = (dynamic)dataGrid.SelectedItem;
                    int userIdToDelete = selectedUser.UserId;

                    var confirmResult = MessageBox.Show(
                        $"Вы уверены, что хотите удалить пользователя {selectedUser.Surname} {selectedUser.Name}?\nЭто действие нельзя отменить.",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var userToDelete = _context.Users.Find(userIdToDelete);
                            if (userToDelete != null)
                            {
                                _context.Users.Remove(userToDelete);
                                _context.SaveChanges();
                                MessageBox.Show("Пользователь успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                window.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };

                grid.Children.Add(dataGrid);
                Grid.SetRow(dataGrid, 0);
                grid.Children.Add(deleteButton);
                Grid.SetRow(deleteButton, 1);

                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRoomsCost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rooms = _context.Rooms
                    .Include(r => r.RoomCategory)
                    .Select(r => new
                    {
                        r.RoomId,
                        CategoryName = r.RoomCategory.RoomCategoryName,
                        Cost = r.RoomCategory.Cost,
                        Description = r.RoomCategory.Description
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Стоимость номеров",
                    Width = 800,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = rooms
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID номера", Binding = new System.Windows.Data.Binding("RoomId"), Width = 80 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new System.Windows.Data.Binding("CategoryName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Стоимость", Binding = new System.Windows.Data.Binding("Cost", StringFormat = "{0:N0} ₽"), Width = 120 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new System.Windows.Data.Binding("Description") });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStaffInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var staff = _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Position)
                    .Include(e => e.Shift)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        FullName = $"{e.User.Surname} {e.User.Name} {e.User.Patronymic}",
                        PositionName = e.Position != null ? e.Position.PositionName : "Не указана",
                        ShiftTime = e.Shift != null ? e.Shift.ShiftTime.ToString() : "Не указана",
                        Salary = e.Salary
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Информация о сотрудниках",
                    Width = 900,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = staff
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("EmployeeId"), Width = 60 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Должность", Binding = new System.Windows.Data.Binding("PositionName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Смена", Binding = new System.Windows.Data.Binding("ShiftTime"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Зарплата", Binding = new System.Windows.Data.Binding("Salary", StringFormat = "{0:N0} ₽"), Width = 100 });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConflicts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var penalties = _context.Penalties
                    .Include(p => p.Employee)
                    .ThenInclude(e => e.User)
                    .Select(p => new
                    {
                        p.PenaltyId,
                        FullName = $"{p.Employee.User.Surname} {p.Employee.User.Name} {p.Employee.User.Patronymic}",
                        p.Amount,
                        p.Reason,
                        p.IssueDate,
                        p.IsPaid
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Конфликты и штрафы",
                    Width = 900,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = penalties
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID штрафа", Binding = new System.Windows.Data.Binding("PenaltyId"), Width = 80 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Сотрудник", Binding = new System.Windows.Data.Binding("FullName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new System.Windows.Data.Binding("Amount", StringFormat = "{0:N0} ₽"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Причина", Binding = new System.Windows.Data.Binding("Reason") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new System.Windows.Data.Binding("IssueDate"), Width = 100 });
                dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Оплачено", Binding = new System.Windows.Data.Binding("IsPaid"), Width = 80 });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuestsInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var guests = _context.Guests
                    .Include(g => g.User)
                    .Include(g => g.Room)
                    .ThenInclude(r => r.RoomCategory)
                    .Include(g => g.PaymentInvoices)
                    .Select(g => new
                    {
                        g.GuestId,
                        FullName = $"{g.User.Surname} {g.User.Name} {g.User.Patronymic}",
                        RoomNumber = g.Room != null ? g.Room.RoomId.ToString() : "Нет",
                        CategoryName = g.Room != null ? g.Room.RoomCategory.RoomCategoryName : "-",
                        TotalBill = g.PaymentInvoices.Sum(pi => pi.Total),
                        IsPaid = g.PaymentInvoices.Any(pi => pi.IsPaid)
                    })
                    .ToList();

                var window = new Window
                {
                    Title = "Информация о гостях",
                    Width = 900,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    Margin = new Thickness(10),
                    ItemsSource = guests
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("GuestId"), Width = 60 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName") });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Номер", Binding = new System.Windows.Data.Binding("RoomNumber"), Width = 80 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new System.Windows.Data.Binding("CategoryName"), Width = 100 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Счет", Binding = new System.Windows.Data.Binding("TotalBill", StringFormat = "{0:N0} ₽"), Width = 100 });
                dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Оплачено", Binding = new System.Windows.Data.Binding("IsPaid"), Width = 80 });

                grid.Children.Add(dataGrid);
                window.Content = grid;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
