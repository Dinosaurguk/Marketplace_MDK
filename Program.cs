using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vulpes0
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string projectRoot = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(Application.StartupPath, @"..\..\"));
            AppDomain.CurrentDomain.SetData("DataDirectory", projectRoot);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AuthForm());
        }
        public static class UserSession
        {
            public static int IdUser { get; set; } = 0;
            public static string Role { get; set; } = "";
            public static string Fio { get; set; } = "";
        }

    }
}
