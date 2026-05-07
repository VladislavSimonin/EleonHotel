using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EleonHotel.Windows.MainWindows.Admin
{
    /// <summary>
    /// Логика взаимодействия для Conflicts.xaml
    /// </summary>
    public partial class Conflicts : Window
    {
        private string ConnectionString = "Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;";
        private DataRowView _selectedRow = null;

        public Conflicts()
        {
            InitializeComponent();
            LoadPenaltiesData();
        }

        private void LoadPenaltiesData()
        {
            string query = @"
                SELECT
                    p.penalty_id,
                    u.surname,
                    u.name,
                    u.patronymic,
                    p.amount,
                    p.reason,
                    p.issue_date,
                    p.is_paid
                FROM Penalties p
                INNER JOIN Users u ON p.user_id = u.user_id
                ORDER BY p.issue_date DESC";

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        PenaltiesDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PenaltiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PenaltiesDataGrid.SelectedItem != null)
            {
                _selectedRow = PenaltiesDataGrid.SelectedItem as DataRowView;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPenaltyWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                LoadPenaltiesData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow == null)
            {
                MessageBox.Show("Выберите штраф для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int penaltyId = Convert.ToInt32(_selectedRow["penalty_id"]);
            var editWindow = new EditPenaltyWindow(penaltyId);
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                LoadPenaltiesData();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
