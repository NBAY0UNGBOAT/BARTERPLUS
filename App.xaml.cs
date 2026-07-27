using System.Windows;

namespace BarterPOS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Handle unhandled exceptions globally
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    $"An error occurred: {args.Exception.Message}\n\n{args.Exception.StackTrace}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}
