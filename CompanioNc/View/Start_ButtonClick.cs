﻿using System;
using System.Windows;

namespace CompanioNc.View
{

    /// <summary>
    /// 20210717建立
    /// </summary>
    public partial class Start : Window
    {
        private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();
    }
}
