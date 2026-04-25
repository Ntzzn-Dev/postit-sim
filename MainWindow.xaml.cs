using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace postitSimulator
{
    public partial class MainWindow : Window
    {
        private bool isDocked = false;
        private double normalWidth = 250;
        private double normalHeigth = 300;

        public MainWindow()
        {
            InitializeComponent();
        }

        // DRAG
        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDocked)
                this.DragMove();
        }

        // AUTO TÍTULO
        private void TextPost_changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var firstLine = TextPost.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .FirstOrDefault();

            TitlePost.Text = string.IsNullOrWhiteSpace(firstLine)
                ? "Post-it"
                : firstLine;
        }

        // FECHAR
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        { 
            this.Close();
        }

        // DETECTA SOLTAR MOUSE (para dock)
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDocked) return;

            var screenWidth = SystemParameters.WorkArea.Width;

            double threshold = 7;

            if (this.Left <= threshold)
            {
                DockLeft();
            } else if (this.Left + this.Width >= screenWidth - threshold){
                DockRight();
            }
        }

        // CLIQUE NA JANELA (restaurar)
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDocked)
            {
               // Undock_Click();
            }
        }

        // DOCK ESQUERDA
        private void DockLeft()
        {
            isDocked = true;

            normalWidth = this.Width;
            normalHeigth = this.Height;

            this.Left = 0;
            this.Width = 30;
            this.Height = 80;

            NormalView.Visibility = Visibility.Collapsed;
            DockView.Visibility = Visibility.Visible;

            DockedPath.Data = Geometry.Parse("M-10,-5 C15,20 29.75,20 30,40 C29.75,60 15,60 -10,85 C-10,60 -10,20 -10,-5 Z");
        }

        // DOCK DIREITA
        private void DockRight()
        {
            isDocked = true;
            var screenWidth = SystemParameters.WorkArea.Width;

            normalWidth = this.Width;
            normalHeigth = this.Height;

            this.Left = screenWidth - 30;
            this.Width = 30;
            this.Height = 80;

            NormalView.Visibility = Visibility.Collapsed;
            DockView.Visibility = Visibility.Visible;

            DockedPath.Data = Geometry.Parse("M40,-5 C15,20 1.25,20 0,40 C1.25,60 15,60 40,85 C40,60 40,20 40,-5 Z");
        }
        
        // UNDOCK
        private void Undock_Click(object sender, MouseButtonEventArgs e)
        {
            isDocked = false;

            var screenWidth = SystemParameters.WorkArea.Width;

            this.Left = this.Left == 0 ? 8 : screenWidth - normalWidth - 8;
            this.Width = normalWidth;
            this.Height = normalHeigth;

            NormalView.Visibility = Visibility.Visible;
            DockView.Visibility = Visibility.Collapsed;
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var menu = (ContextMenu)this.Resources["GlobalMenu"];
            menu.PlacementTarget = this;
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;
        }

        private void Color_Change_Click(object sender, RoutedEventArgs e)
        {
            NormalViewHeader.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(DarkPostItColors[_colorIndex]));
            var color = GetNextPostItColor(PostItColors);
            NormalView.Background = color;
            DockedPath.Fill = color;
        }

        private static readonly String[] PostItColors =
        {
            "#7FF4FFF1",
            "#FFFDBA74",
            "#FFFFC1CC",
            "#FFBEEBC1",
            "#FFCBE7FF",
            "#FFF9F17F",
        };
        private static readonly String[] DarkPostItColors =
        {
            "#5FCFE6D6",
            "#FFE6A85F",
            "#FFE09CAA",
            "#FF9FD49F",
            "#FFA9C8E6",
            "#FFEFE066",
        };

        private static int _colorIndex = 0;

       private SolidColorBrush GetNextPostItColor(String[] Colors)
        {
            var colorStr = Colors[_colorIndex];

            _colorIndex++;

            if (_colorIndex >= Colors.Length)
                _colorIndex = 0;

            var color = (Color)ColorConverter.ConvertFromString(colorStr);
            return new SolidColorBrush(color);
        }

        private void Opcao2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Excluir clicado!");
        }
    }
}