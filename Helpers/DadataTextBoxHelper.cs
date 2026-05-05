using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EleonHotel.Data.Classes;

namespace EleonHotel.Helpers
{
    /// <summary>
    /// Помощник для подключения автодополнения DaData к TextBox
    /// </summary>
    public class DadataTextBoxHelper
    {
        private readonly DadataService _dadataService;
        private readonly TextBox _textBox;
        private readonly PopupType _popupType;
        private Popup _popup;
        private ListBox _listBox;
        private bool _isUpdating;

        public enum PopupType
        {
            Address,
            Fio,
            Phone,
            Email
        }

        public DadataTextBoxHelper(TextBox textBox, PopupType type)
        {
            _textBox = textBox;
            _popupType = type;
            _dadataService = new DadataService();

            SetupAutoComplete();
        }

        private void SetupAutoComplete()
        {
            // Создаем Popup для отображения подсказок
            _popup = new Popup
            {
                PlacementTarget = _textBox,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true
            };

            _listBox = new ListBox
            {
                Width = _textBox.ActualWidth > 0 ? _textBox.ActualWidth : 300,
                MaxHeight = 200,
                BorderThickness = new Thickness(1),
                BorderBrush = System.Windows.Media.Brushes.Gray
            };

            _listBox.MouseLeftButtonUp += ListBox_MouseLeftButtonUp;
            _listBox.KeyDown += ListBox_KeyDown;

            _popup.Child = _listBox;

            // Подписываемся на события TextBox
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.LostFocus += TextBox_LostFocus;
            _textBox.GotFocus += TextBox_GotFocus;
            _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            var query = _textBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                _popup.IsOpen = false;
                return;
            }

            List<string> suggestions = null;

            try
            {
                switch (_popupType)
                {
                    case PopupType.Address:
                        suggestions = await _dadataService.GetAddressSuggestionsAsync(query);
                        break;
                    case PopupType.Fio:
                        suggestions = await _dadataService.GetFioSuggestionsAsync(query);
                        break;
                    case PopupType.Phone:
                        suggestions = await _dadataService.GetPhoneSuggestionsAsync(query);
                        break;
                    case PopupType.Email:
                        suggestions = await _dadataService.GetEmailSuggestionsAsync(query);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения подсказок: {ex.Message}");
                return;
            }

            if (suggestions != null && suggestions.Count > 0)
            {
                _listBox.ItemsSource = suggestions;
                _popup.IsOpen = true;
            }
            else
            {
                _popup.IsOpen = false;
            }
        }

        private void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_listBox.SelectedItem != null)
            {
                _isUpdating = true;
                _textBox.Text = _listBox.SelectedItem.ToString();
                _textBox.CaretIndex = _textBox.Text.Length;
                _popup.IsOpen = false;
                _isUpdating = false;
            }
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _listBox.SelectedItem != null)
            {
                _isUpdating = true;
                _textBox.Text = _listBox.SelectedItem.ToString();
                _textBox.CaretIndex = _textBox.Text.Length;
                _popup.IsOpen = false;
                _isUpdating = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _popup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_popup.IsOpen) return;

            if (e.Key == Key.Down)
            {
                if (_listBox.Items.Count > 0)
                {
                    _listBox.SelectedIndex = (_listBox.SelectedIndex + 1) % _listBox.Items.Count;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (_listBox.Items.Count > 0)
                {
                    _listBox.SelectedIndex = _listBox.SelectedIndex <= 0 
                        ? _listBox.Items.Count - 1 
                        : _listBox.SelectedIndex - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (_listBox.SelectedItem != null)
                {
                    _isUpdating = true;
                    _textBox.Text = _listBox.SelectedItem.ToString();
                    _textBox.CaretIndex = _textBox.Text.Length;
                    _popup.IsOpen = false;
                    _isUpdating = false;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                _popup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Закрываем popup с небольшой задержкой, чтобы успеть кликнуть по элементу
            System.Windows.Threading.DispatcherTimer timer = 
                new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(200)
                };
            timer.Tick += (s, args) =>
            {
                if (!System.Windows.Mouse.LeftButton.Equals(System.Windows.Input.Mouse.PrimaryDevice.LeftButton))
                {
                    _popup.IsOpen = false;
                }
                timer.Stop();
            };
            timer.Start();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Обновляем ширину popup при получении фокуса
            if (_textBox.ActualWidth > 0)
            {
                _listBox.Width = _textBox.ActualWidth;
            }
        }

        /// <summary>
        /// Очистить подписки и ресурсы
        /// </summary>
        public void Cleanup()
        {
            _textBox.TextChanged -= TextBox_TextChanged;
            _textBox.LostFocus -= TextBox_LostFocus;
            _textBox.GotFocus -= TextBox_GotFocus;
            _textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
            _listBox.MouseLeftButtonUp -= ListBox_MouseLeftButtonUp;
            _listBox.KeyDown -= ListBox_KeyDown;
        }
    }
}
