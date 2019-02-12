using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Hardware;
using System.Runtime.InteropServices;
using Firmware;
using CommandTypes;
using CommandHandler;
using System.Diagnostics;

namespace PrinterSimulator
{
    class CommTest
    {
        [STAThread]

        static void Main()
        {

            // Start the printer - DO NOT CHANGE THESE LINES
            PrinterThread printer = new PrinterThread();
            Thread oThread = new Thread(new ThreadStart(printer.Run));
            oThread.Start();
            printer.WaitForInit();

            // Start the firmware thread - DO NOT CHANGE THESE LINES
            FirmwareController firmware = new FirmwareController(printer.GetPrinterSim());
            oThread = new Thread(new ThreadStart(firmware.Start));
            oThread.Start();
            firmware.WaitForInit();

            Command command = new Command(printer.GetPrinterSim());

			int goodCounter = 0;
			for (int i = 0; i < 100; i++)
			{
				
				string result = command.SendCommandParam1(FWCommands.CMD_SET_LASER, 1);

				if (result == "SUCCESS") goodCounter++;
			
			}
			
			if (goodCounter == 100)
				Console.WriteLine("Test passed");
			else
				Console.WriteLine("Test failed");
			
            printer.Stop();
            firmware.Stop();

        }
    }
}