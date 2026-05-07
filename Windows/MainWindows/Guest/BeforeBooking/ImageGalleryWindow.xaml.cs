using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EleonHotel.Windows.MainWindows
{
    public partial class ImageGalleryWindow : Window
    {
        private List<string> images = new List<string>();
        private int currentIndex = 0;

        public ImageGalleryWindow(List<string> imagesList, string roomCategory = "Номер")
        {
            InitializeComponent();
            images = imagesList ?? new List<string>();
            TblTitle.Text = string.Format("Просмотр изображений - {0}", roomCategory);
            LoadImage(0);
            LoadThumbnails();
        }

        private void LoadImage(int index)
        {
            if (images.Count == 0)
                return;

            currentIndex = (index % images.Count + images.Count) % images.Count;

            try
            {
                var uri = new Uri(images[currentIndex], UriKind.RelativeOrAbsolute);
                MainImage.Source = new BitmapImage(uri);
                TblCounter.Text = string.Format("{0} / {1}", currentIndex + 1, images.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка при загрузке изображения: {0}", ex.Message), "Ошибка");
            }
        }

        private void LoadThumbnails()
        {
            ThumbnailsPanel.Children.Clear();

            for (int i = 0; i < images.Count; i++)
            {
                var thumbnail = CreateThumbnail(i);
                ThumbnailsPanel.Children.Add(thumbnail);
            }
        }

        private Button CreateThumbnail(int index)
        {
            var btn = new Button
            {
                Width = 100,
                Height = 100,
                Margin = new Thickness(5),
                Background = null,
                BorderThickness = new Thickness(2),
                BorderBrush = index == currentIndex ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.LightGray,
                Cursor = Cursors.Hand
            };

            try
            {
                var img = new Image
                {
                    Stretch = System.Windows.Media.Stretch.UniformToFill,
                    Source = new BitmapImage(new Uri(images[index], UriKind.RelativeOrAbsolute))
                };
                btn.Content = img;
            }
            catch { }

            btn.Click += (s, e) => LoadImage(index);
            return btn;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(currentIndex + 1);
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(currentIndex - 1);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                case Key.D:
                    BtnNext_Click(null, null);
                    break;
                case Key.Left:
                case Key.A:
                    BtnPrevious_Click(null, null);
                    break;
                case Key.Escape:
                    Close();
                    break;
            }
            base.OnKeyDown(e);
        }
    }
}
