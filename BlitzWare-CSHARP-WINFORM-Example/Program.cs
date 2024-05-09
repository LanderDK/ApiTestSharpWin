using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlitzWare
{
    internal static class Program
    {
        public static API BlitzWareAuth = new(
            apiUrl: "https://api.blitzware.xyz/api",
            appName: "NAME",
            appSecret: "SECRET",
            appVersion: "VERSION"
        );

        [STAThread]
        static void Main()
        {
            BlitzWareAuth.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }
    }
}
