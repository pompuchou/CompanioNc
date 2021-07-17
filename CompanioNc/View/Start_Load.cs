using GlobalHotKey;
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
        public HotKeyManager hotKeyManager;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Create the hotkey manager.
            hotKeyManager = new HotKeyManager();

            // Register Ctrl+F2 hotkey. Save this variable somewhere for the further unregistering.
            try
            {
                hotKeyManager.Register(Key.F2, ModifierKeys.Control);
                hotKeyManager.Register(Key.T, ModifierKeys.Control);
                log.Info("Hotkey F2, Ctrl-T registered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Register Ctrl-F2, Ctrl-T. Fatal. Error: {ex.Message}");
                this.Close();
            }

            // Handle hotkey presses.
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }
    }
}
