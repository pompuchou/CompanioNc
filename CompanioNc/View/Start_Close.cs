using System;
using System.Windows;
using System.Windows.Input;

namespace CompanioNc.View
{
    /// <summary>
    /// 20210717建立
    /// </summary>
    public partial class Start : Window
    {
        private void Window_Closed(object sender, EventArgs e)
        {
            // Unregister Ctrl+Alt+F2 hotkey.
            try
            {
                hotKeyManager.Unregister(Key.F2, ModifierKeys.Control);
                hotKeyManager.Unregister(Key.T, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-F2, Ctrl-T unregistered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Unregister Ctrl-F2, Ctrl-T. Fatal. Error: {ex.Message}");
            }

            // Dispose the hotkey manager.
            hotKeyManager.Dispose();
        }

    }
}
