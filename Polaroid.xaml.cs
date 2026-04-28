using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace postitSimulator
{
    public partial class Polaroid : Window
    {
        private int isDockedLR = 0;
        private double normalWidth = 250;
        private double normalHeigth = 300;
        private double aspectRatio;

        public Polaroid()
        {
            InitializeComponent();
            aspectRatio = this.Width / this.Height;
        }

        private bool _isResizing = false;
        private Point _startMouse;
        private double _startWidth;
        private double _startHeight;

       private void ResizeStart(object sender, MouseButtonEventArgs e)
        {
            _isResizing = true;
            _startMouse = e.GetPosition(null);

            _startWidth = this.Width;
            _startHeight = this.Height;

            Mouse.Capture((UIElement)sender);

            e.Handled = true; // 🔥 ISSO AQUI resolve o conflito
        }

        private void ResizeMove(object sender, MouseEventArgs e)
        {
            if (!_isResizing) return;

            var current = e.GetPosition(null);
            var dx = current.X - _startMouse.X;

            double newWidth = _startWidth + dx;

            if (newWidth < 100) return; // evita quebrar layout

            double newHeight = newWidth / aspectRatio;

            this.Width = newWidth;
            this.Height = newHeight;

            e.Handled = true; // 🔥 importante
        }

        private void ResizeEnd(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            Mouse.Capture(null);

            e.Handled = true;
        }

        // Movimentação : =================================

        private Point _startPoint;
        private bool _isDragging = false;
        private const double DragThreshold = 5;

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void DragArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var current = e.GetPosition(null);

            if (!_isDragging){
                var dx = current.X - _startPoint.X;
                var dy = current.Y - _startPoint.Y;

                if (Math.Sqrt(dx * dx + dy * dy) > DragThreshold)
                {
                    _isDragging = true;
                }
            }

            var screenWidth = SystemParameters.WorkArea.Width;

            if (_isDragging){
                this.DragMove();
                if (isDockedLR == 1)
                    this.Left = 0;
                else if (isDockedLR == 2)
                    this.Left = screenWidth - 40;
            }
        }
        
        private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging && isDockedLR != 0)
            {
                Undock();
            }

            _isDragging = false;
        }

        // Dock : =========================================

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Title = "Escolher imagem";
            dialog.Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                var imagePath = dialog.FileName;

                MyImage.Source = new BitmapImage(new Uri(imagePath));
            }
        }

        // DETECTA SOLTAR MOUSE (para dock)
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDockedLR != 0) return;

            var screenWidth = SystemParameters.WorkArea.Width;

            double threshold = -10;

            if (this.Left <= threshold)
            {
                DockLeft();
            } else if (this.Left + this.Width >= screenWidth - threshold){
                DockRight();
            }
        }

        // DOCK ESQUERDA
        private void DockLeft()
        {
            isDockedLR = 1;

            normalWidth = this.Width;
            normalHeigth = this.Height;

            this.Left = 0;
            this.Width = 40;
            this.Height = 100;

            NormalView.Visibility = Visibility.Collapsed;
            DockView.Visibility = Visibility.Visible;

            DockedPath.Data = Geometry.Parse("M-10,-5 C15,20 29.75,20 30,40 C29.75,60 15,60 -10,85 C-10,60 -10,20 -10,-5 Z");
        }

        // DOCK DIREITA
        private void DockRight()
        {
            isDockedLR = 2;
            var screenWidth = SystemParameters.WorkArea.Width;

            normalWidth = this.Width;
            normalHeigth = this.Height;

            this.Left = screenWidth - 40;
            this.Width = 40;
            this.Height = 100;

            NormalView.Visibility = Visibility.Collapsed;
            DockView.Visibility = Visibility.Visible;

            DockedPath.Data = Geometry.Parse("M50,-5 C25,20 11.25,20 10,40 C11.25,60 25,60 50,85 C50,60 50,20 50,-5 Z");
        }
        
        // UNDOCK
        private void Undock()
        {
            isDockedLR = 0;

            var screenWidth = SystemParameters.WorkArea.Width;

            this.Left = this.Left != 0 ? screenWidth - normalWidth : 0;
            this.Width = normalWidth;
            this.Height = normalHeigth;

            NormalView.Visibility = Visibility.Visible;
            DockView.Visibility = Visibility.Collapsed;
            AnimateScale(1.0);
        }

        // Menu : =========================================

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var menu = (ContextMenu)this.Resources["GlobalMenu"];
            menu.PlacementTarget = this;
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;
        }

        private void WriteDetail_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("");

            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                string texto = dialog.Result;

                DetailsText.Text = texto;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Animação : =====================================

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isDockedLR == 0) return;
            AnimateScale(1.2);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateScale(1.0);
        }

        private void AnimateScale(double scale)
        {
            var anim = new DoubleAnimation
            {
                To = scale,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase()
            };

            WindowScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            WindowScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void AddPostit_Click(object sender, RoutedEventArgs e)
        {
            new Postit().Show();
        }
        private void AddPolaroid_Click(object sender, RoutedEventArgs e)
        {
            new Polaroid().Show();
        }
    }
}