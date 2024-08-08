using System;
using System.Windows.Forms;

using Nevron.Nov.Barcode;
using Nevron.Nov.Text;
using Nevron.Nov.Windows.Forms;

namespace Nevron.Nov.Diagram.Converter
{
    static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Install Nevron Open Vision for Windows Forms
			NNovApplicationInstaller.Install(
				NTextModule.Instance,
				NDiagramModule.Instance,
				NBarcodeModule.Instance);

			// Configure the application to run in developer mode, so that expressions and additional
			// properties are visible in the designers
			NApplication.DeveloperMode = true;
            //NApplication.EnableGPURendering = false;

			// Run the main form of the application
			Application.Run(NDiagramConverter.CreateForm());
		}
	}
}