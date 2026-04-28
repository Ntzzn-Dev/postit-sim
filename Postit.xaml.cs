using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace postitSimulator
{
    public partial class Postit : Window
    {
        private int isDockedLR = 0;
        private double normalWidth = 250;
        private double normalHeigth = 300;
        private String normalTitle = "";
        private String normalPath = "";

        public Postit()
        {
            InitializeComponent();

            PostitData.ReadJSON();

            LoadPostit(PostitData.banco.FirstOrDefault(x => x.Titulo == TitlePost.Text));
        }

        public Postit(String title, String content, String path)
        {
            InitializeComponent();

            TextPost.Text = content;
            TitlePost.Text = title;
            DockView.ToolTip = title;
            normalPath = path;

            LoadPostit(PostitData.banco.FirstOrDefault(x => x.Titulo == title));
        }

        private void LoadPostit(PostitData postitSalvo)
        {
            if (postitSalvo == null) return;

            SetColor(postitSalvo.Cor);
            SetIcon(postitSalvo.Icone);
            SetPosY(postitSalvo.PosicaoY);

            if (postitSalvo.LadoLR == 1)
                DockLeft();
            else if (postitSalvo.LadoLR == 2)
                DockRight();
        }

        private void SavePostit()
        {
            var postitSalvo = PostitData.banco
                .FirstOrDefault(x => x.Titulo == TitlePost.Text);

            if (postitSalvo != null)
            {
                postitSalvo.Icone = IconPost.Text;
                postitSalvo.PosicaoY = this.Top;
                postitSalvo.LadoLR = isDockedLR;
                postitSalvo.Cor = DockedPath.Tag is int c ? c : 0;
            }
            else
            {
                PostitData.banco.Add(new PostitData(
                    TitlePost.Text,
                    IconPost.Text,
                    this.Top,
                    isDockedLR,
                    DockedPath.Tag is int c ? c : 0
                ));
            }

            PostitData.SaveJSON();
        }

        private void SetColor(int num){
            NormalViewHeader.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(DarkPostItColors[num]));
            NormalView.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(PostItColors[num]));
            DockedPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(PostItColors[num]));
            DockedPath.Tag = num;
        }
        private void SetIcon(String ic){
            IconPost.Text = ic;
        }
        private void SetPosY(double py){
            this.Top = py;
        }
    
        private static readonly String[] PostItColors =
        {
            "#FFF9F17F",
            "#7FF4FFF1",
            "#FFFDBA74",
            "#FFFFC1CC",
            "#FFBEEBC1",
            "#FFCBE7FF",
            "#FFFC9F9F"
        };
        private static readonly String[] DarkPostItColors =
        {
            "#FFEFE066",
            "#5FCFE6D6",
            "#FFE6A85F",
            "#FFE09CAA",
            "#FF9FD49F",
            "#FFA9C8E6",
            "#FFE06A6A"
        };

        // Movimentação : =================================

        private Point _startPoint;
        private bool _isDragging = false;
        private const double DragThreshold = 5;
        private double _startTop;
        private double _startMouseY;

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _startMouseY = e.GetPosition(null).Y;
            _isDragging = false;

            Mouse.Capture((UIElement)sender);
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

            if (_isDragging){
                if(isDockedLR == 0){
                    this.DragMove();
                } else {
                    double currentMouseY = e.GetPosition(null).Y;
                    double deltaY = currentMouseY - _startMouseY;

                    this.Top += deltaY;
                }
            }
        }
        
        private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging && isDockedLR != 0)
            {
                Undock();
            }

            _isDragging = false;
            Mouse.Capture(null);
        }

        // AUTO TÍTULO
        private void TextPost_changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(normalTitle)) return;

            var firstLine = TextPost.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .FirstOrDefault();

            string titulo = string.IsNullOrWhiteSpace(firstLine)
                ? "Post-it"
                : firstLine;

            TitlePost.Text = titulo;
            DockView.ToolTip = titulo;
            this.Title = titulo;
        }

        // DOCKS : ========================================

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
        
        // DOCK AUTOMATICO COM UM CLIQUE
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            var screenWidth = SystemParameters.WorkArea.Width;
            if(this.Left + normalWidth / 2 > screenWidth / 2)
                DockRight();
            else 
                DockLeft();
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

        // MENU / BOTÕES DE AÇÃO : ========================
        
        // ABRIR MENU DE AÇÕES
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var menu = (ContextMenu)this.Resources["GlobalMenu"];
            menu.PlacementTarget = this;
            menu.Placement = PlacementMode.MousePoint;
            menu.IsOpen = true;
        }

        // FECHAR
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        { 
            this.Close();
        }
        
        // ABRIR AQUIVOS DE TEXTO
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        { 
            var dialog = new OpenFileDialog();

            dialog.Title = "Escolher imagem";
            dialog.Filter = "Arquivo de texto (*.txt)|*.txt";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == true) {
                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    var arqv = dialog.FileNames[i];

                    string texto = File.ReadAllText(arqv);
                    var linhas = texto.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    string content = string.Join(Environment.NewLine, linhas);
                    string titulo = Path.GetFileNameWithoutExtension(arqv);

                    normalPath = arqv;
                    normalTitle = titulo;

                    if (i == 0)
                    {
                        TextPost.Text = content;
                        TitlePost.Text = titulo;
                        DockView.ToolTip = titulo;
                        LoadPostit(PostitData.banco.FirstOrDefault(x => x.Titulo == titulo));
                    }
                    else
                    {
                        new Postit(titulo, content, normalPath).Show();
                    }
                }
            }
        }

        // SALVAR AQUIVOS DE TEXTO
        private void SaveAsBtn_Click(object sender, RoutedEventArgs e) => SaveAs();
        
        private void SaveBtn_Click(object sender, RoutedEventArgs e) => SaveTXT();

        private void SaveShortcut_Executed(object sender, ExecutedRoutedEventArgs e) => SaveTXT();

        private void SaveTXT(){
            if(string.IsNullOrWhiteSpace(normalPath))
                SaveAs();
            else{
                string conteudo = TextPost.Text;

                File.WriteAllText(normalPath, conteudo);
            }
        }

        private void SaveAs()
        {
            var dialog = new SaveFileDialog();

            dialog.Title = "Salvar arquivo";
            dialog.Filter = "Arquivo de texto (*.txt)|*.txt";
            dialog.DefaultExt = ".txt";

            if (dialog.ShowDialog() == true)
            {
                string caminho = dialog.FileName;

                string conteudo = TextPost.Text;

                normalPath = caminho;

                File.WriteAllText(normalPath, conteudo);
                
                string titulo = Path.GetFileNameWithoutExtension(caminho);
                TitlePost.Text = titulo;
                normalTitle = titulo;
            }
        }

        // ADICIONAR COLAGENS NA TELA
        private void AddPostit_Click(object sender, RoutedEventArgs e)
        {
            new Postit().Show();
        }
        private void AddPolaroid_Click(object sender, RoutedEventArgs e)
        {
            new Polaroid().Show();
        }

        // SELEÇÕES
        private void IconSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SetIcon(btn.Content?.ToString());
                SavePostit();
            }
        }
        private void ColorSelect_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            if (sender is Button btn && btn.Tag is string tag)
            {
                index = int.Parse(tag);
            }
            SetColor(index);
            SavePostit();
        }

        // APAGAR TEXTO
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextPost.Text))
                this.Close();
            else
                TextPost.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => SavePostit();

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
    }
}