﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace trafficInformation.Logics
{
    public class Commons
    {
        public static readonly string myConnString = "Server=localhost;" +
                                                     "Port=3306;" +
                                                     "Database=miniproject;" +
                                                     "Uid=root;" +
                                                     "Pwd=815301;" +
                                                     "allow user variables=true";
        public static async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, message, style, null);
        }
    }

}
