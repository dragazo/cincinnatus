using System;
using System.Windows.Forms;

namespace Cincinnatus
{
	static class Program
	{
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
			else Console.Error.WriteLine("Too many arguments");
		}
	}
}
