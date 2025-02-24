using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace THJPatcher
{
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "THJInstallerSingleInstance";
            bool createdNew;

            try
            {
                _mutex = new Mutex(true, appName, out createdNew);

                if (!createdNew)
                {
                    MessageBox.Show("The application is already running.");
                    Current.Shutdown();
                    return;
                }
                var splash = new Views.SplashScreen();
                splash.Show();

                var mainWindow = new MainWindow();
                MainWindow = mainWindow;

                Task.Run(() =>
                {
                    LoadDataOrOtherTasks();
                }).ContinueWith((task) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        splash.Close();
                        mainWindow.Show();
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup error: {ex.Message}");
                Current.Shutdown();
            }
        }

        private void LoadDataOrOtherTasks()
        {
           
            Thread.Sleep(2000);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
