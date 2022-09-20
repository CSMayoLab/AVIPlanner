using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnalyticsLibrary2
{
    public static class ConsoleExt
    {
        public static void WriteLineWithBackground(string msg, ConsoleColor bg_color = ConsoleColor.DarkYellow)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = bg_color;
            Console.Write(msg);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" \n");
        }

        public static MessageBoxResult WriteLine_n_Messagebox(string msg)
        {
            Console.WriteLine(msg);
            var msgboxres = MessageBox.Show(msg);
            return msgboxres;
        }
    }

}
