using System.Windows;
using System.Windows.Input;

namespace postitSimulator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextPost_changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = TextPost.Text;

            var firstLine = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .FirstOrDefault();

            TitlePost.Text = string.IsNullOrWhiteSpace(firstLine)
                ? "Post-it"
                : firstLine;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}