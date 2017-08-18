using System;
using JetBrains.Annotations;
using Microsoft.Win32;
using Serilog;

namespace homeControl.Client.WPF
{
    [UsedImplicitly]
    public sealed class AutorunConfigurator
    {
        private readonly ILogger _log;
#if !DEBUG
        private const string KeyName = "HomeControl";
#else
        private const string KeyName = "HomeControlDebug";
#endif

        private string GetProgramPath()
        {
            return
#if !DEBUG
                Environment.GetFolderPath(Environment.SpecialFolder.Programs) + @"\svtz\homeControl\Дом.appref-ms";
#else
                System.Reflection.Assembly.GetExecutingAssembly().Location;
#endif
        }

        public AutorunConfigurator(ILogger log)
        {
            Guard.DebugAssertArgumentNotNull(log, nameof(log));
            _log = log;
        }

        private RegistryKey OpenAutorunKey()
        {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        }

        private void ExecuteIgnoringExceptions(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error while configuring autorun.");
            }
        }

        public void SetupAutoRun()
        {
            _log.Debug("Enabling autorun");
            ExecuteIgnoringExceptions(() =>
            {
                using (var rkApp = OpenAutorunKey())
                {
                    if (rkApp == null)
                        return;

                    var startPath = GetProgramPath();
                    rkApp.SetValue(KeyName, startPath);
                }
            });
        }

        public void RemoveAutoRun()
        {
            _log.Debug("Disabling autorun");
            ExecuteIgnoringExceptions(() =>
            {
                using (var rkApp = OpenAutorunKey())
                {
                    if (rkApp == null)
                        return;

                    rkApp.DeleteValue(KeyName);
                }
            });
        }
    }
}