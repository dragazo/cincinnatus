using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Cincinnatus
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 0) Application.Run(new PhotoViewer());
            else if (args.Length == 1)
            {
                PhotoViewer viewer = new PhotoViewer();
                if (viewer.SetImage(args[0])) Application.Run(viewer);
            }
            else for (int i = 0; i < args.Length; i++) LaunchProcess(args[i]);
        }

        /// <summary>
        /// Launches a new process, passing path as its execution argument
        /// </summary>
        /// <param name="path"></param>
        public static void LaunchProcess(string path)
        {
            Process.Start(new ProcessStartInfo(Application.ExecutablePath, path));
        }
        /// <summary>
        /// Launches a new process with no execution arguments passed
        /// </summary>
        public static void LaunchProcess()
        {
            Process.Start(Application.ExecutablePath);
        }
    }
}
