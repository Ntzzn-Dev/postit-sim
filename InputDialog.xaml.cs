using System.Windows;
using System.Windows.Input;

namespace postitSimulator
{
    public partial class InputDialog : Window
    {
        public string Result { get; private set; }

        public InputDialog(string initialText = "")
        {
            InitializeComponent();
            InputBox.Text = initialText;
            InputBox.Focus();
            InputBox.SelectAll();
            Result="";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = InputBox.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

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
            }
        }
        
        private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }
    }
}