#define USE_OPTIMAL_COMMS

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
using GCoder;
using CommandHandler;
using CommandTypes;
using SimTests;
using System.Diagnostics;


namespace PrinterSimulator
{
    class PrintSim
    {

        const int CMD_SEND_THRESHOLD = 47;

        static void PrintFile(string fileName, Command command, PrinterControl simCtl)
        {
            string Cmd;
            float X, Y, Z, E;
            bool fLaserOn = false;

            command.SendSingleCommand((byte)FWCommands.CMD_RETRACT_PLATE); // Retract plate

            Stopwatch swTimer = new Stopwatch();
            swTimer.Start();

            command.SendCommandParam1((byte)FWCommands.CMD_REMOVE_MODEL, 0); // Set Z axis
            command.SendCommandParam1((byte)FWCommands.CMD_SET_Z, 0); // Set Z axis

            command.StartCommandBuffer();

            GCode coder = new GCode(fileName);

            Cmd = ""; X = 0; Y = 0; Z = 0; E = 0;

            while (coder.GetNextLine(ref Cmd, ref X, ref Y, ref Z, ref E)) {

#if USE_OPTIMAL_COMMS
                if (Z != 0)
                {
                    command.Add((byte)FWCommands.CMD_SET_Z);
                    command.Add(4);
                    command.Add(Z);
                }

                if (E != 0)  // Turn on laser
                {
                    if (!fLaserOn)
                    {
                        command.Add((byte)FWCommands.CMD_SET_LASER);
                        command.Add(1);
                        command.Add(1);

                        fLaserOn = true;
                    }
                }
                else  // Turn OFF laser
                {
                    if (fLaserOn)
                    {
                        command.Add((byte)FWCommands.CMD_SET_LASER);
                        command.Add(1);
                        command.Add(0);

                        fLaserOn = false;
                    }

                }

                if (X != 0 || Y != 0)
                {
                    command.Add((byte)FWCommands.CMD_MOVE_GALVOS);
                    command.Add(8);
                    command.Add(X * 0.025f);
                    command.Add(Y * 0.025f);

                }

                if (command.Count() > CMD_SEND_THRESHOLD)
                {
                    command.SendCommandBuffer();
                    command.StartCommandBuffer();
                }
#else

                if (Z != 0)
                    command.SendCommandParam1((byte)FWCommands.CMD_SET_Z, Z); // Set Z axis

                if (E != 0) {  // Set laser
                    if (!fLaserOn)
                    {
                        command.SendCommandParam1((byte)FWCommands.CMD_SET_LASER, 1);

                        fLaserOn = true;
                    }
                }
                else  // Turn OFF laser
                {
                    if (fLaserOn)
                    {
                        command.SendCommandParam1((byte)FWCommands.CMD_SET_LASER, 0);

                        fLaserOn = false;
                    }

                }

                if (X != 0 || Y != 0)
                    command.SendCommandParam2((byte)FWCommands.CMD_MOVE_GALVOS, X * 0.025f, Y * 0.025f);
#endif
                Cmd = ""; X = 0; Y = 0; Z = 0; E = 0;
            }



            command.SendSingleCommand((byte)FWCommands.CMD_RETRACT_PLATE); // Retract plate  

            swTimer.Stop();

            long elapsedMS = swTimer.ElapsedMilliseconds;

            Console.WriteLine("Total Print Time: {0}", elapsedMS / 1000.0);
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        [STAThread]

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        static void Main()
        {

            IntPtr ptr = GetConsoleWindow();
            //MoveWindow(ptr, -1000, 0, 1000, 400, true);  // Move console window out from under printer windows

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

            Tests testMenu = new Tests(command, printer.GetPrinterSim());

            SetForegroundWindow(ptr);

            bool fDone = false;
            while (!fDone)
            {
                Console.Clear();
                Console.WriteLine("3D Printer Simulation - Control Menu\n");
                Console.WriteLine("P - Print");
                Console.WriteLine("T - Test");
                Console.WriteLine("Q - Quit");

                char ch = Char.ToUpper(Console.ReadKey().KeyChar);
                switch (ch)
                {
                    case 'P': // Print
                        PrintFile("..\\SampleSTLs\\F-35_Corrected.gcode", command, printer.GetPrinterSim());
                        break;

                    case 'T': // Test menu
                        testMenu.TestMenu();
                        break;

                    case 'Q' :  // Quite
                        printer.Stop();
                        firmware.Stop();
                        fDone = true;
                        break;
                }

            }

        }
    }
}