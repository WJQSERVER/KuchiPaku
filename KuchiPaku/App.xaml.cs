////////////////////////////////////////////////////////////////////////////
//
// Epoxy template source code.
// Write your own copyright and note.
// (You can use https://github.com/rubicon-oss/LicenseHeaderManager)
//
////////////////////////////////////////////////////////////////////////////

using System.Globalization;
using System.Threading;
using System.Windows;

namespace KuchiPaku
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set culture based on system settings or specific logic
            // For now, let it follow system culture, which is default behavior,
            // but we can force it here if needed for testing or user settings.
            // var culture = new CultureInfo("zh-CN");
            // Thread.CurrentThread.CurrentCulture = culture;
            // Thread.CurrentThread.CurrentUICulture = culture;
        }

    }
}
