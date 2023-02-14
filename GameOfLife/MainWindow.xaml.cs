using System.Diagnostics;
using System.Windows;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();


        }

        private void btnRegen_Click(object sender, RoutedEventArgs e)
        {
            world.Regen();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            world.Controller.Pause();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            world.Controller.Start();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            world.Controller.Clear();
        }

        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            world.Controller.Step();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            world.LogEvent += (s, ee) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Debug.WriteLine(ee.Payload);
                });
            };
        }
    }
}
