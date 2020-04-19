using System.Windows;
using System.Windows.Controls;

namespace CompanioNc.ViewModels.Selectors
{
    public class MyDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(
            object item,
            DependencyObject container)
        {
            Window wnd = Application.Current.MainWindow;
            if (item is string)
                return wnd.FindResource("WaitTemplate") as DataTemplate;
            else
                return wnd.FindResource("TheItemTemplate") as DataTemplate;
        }
    }
}
