using MediaDeviceApp.View;
using MediaDeviceApp.ViewModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Security;
using System.Windows;

namespace MediaDevicesApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if !NETCOREAPP
        [HandleProcessCorruptedStateExceptions]
#endif
        [SecurityCritical]
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, a) =>
            {
                Exception ex = (Exception)a.ExceptionObject;
                Trace.TraceError(ex.ToString());
                MessageBox.Show(ex.ToString(), "Unhandled Error !!!");
            };

            new MainView() { DataContext = new MainViewModel() }.Show();
        }
    }
}
