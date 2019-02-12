using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandTypes;
using CommandHandler;
using Hardware;

namespace SimTests
{
    class Tests
    {

        Command command;
        PrinterControl printer;
        float curPlateZ = -1.0f;   // Unknown plate Z

        public Tests(Command command, PrinterControl printer)
        {
            this.command = command;
            this.printer = printer;
        }

        void ResetPlate()
        {
            command.SendSingleCommand((byte)FWCommands.CMD_RETRACT_PLATE);
            curPlateZ = printer.GetPrinterHeight();
        }
        public void TestZRailMenu()
        {

            bool fDone = false;
            while (!fDone)
            {
                Console.WriteLine("Z-Rail Test Menu\n");
                Console.WriteLine("  R - Retract plate");
                Console.WriteLine("  Z - Zero plate");
                Console.WriteLine("  D - Lower plate 10 mm");
                Console.WriteLine("  U - Raise plate 10 mm");
//                Console.WriteLine("  X - Reset Z-rail");
                Console.WriteLine("  Q - Quit");

                char ch = Char.ToUpper(Console.ReadKey().KeyChar);
                switch (ch)
                {
                    case 'R': // Retract rail
                        command.SendSingleCommand((byte)FWCommands.CMD_RETRACT_PLATE);
                        break;
                    case 'Z': // Zero plate to resin tray
                        if (curPlateZ < 0) ResetPlate();
                        command.SendCommandParam1((byte)FWCommands.CMD_SET_Z, 0f);
                        curPlateZ = 0;
                        break;
                    case 'D': // Lower plate 10 mm
                        if (curPlateZ >= 0)
                        {
                            command.SendCommandParam1((byte)FWCommands.CMD_SET_Z, curPlateZ - 10.0f);
                            curPlateZ -= 10.0f;
                        }
                        break;
                    case 'U': // Up plate 10 mm
                        if (curPlateZ < 0) ResetPlate();
                        command.SendCommandParam1((byte)FWCommands.CMD_SET_Z, curPlateZ + 10.0f);
                        curPlateZ += 10.0f;
                        break;
                    case 'Q': // Test Z-rail
                        fDone = true;
                        break;
                }
            }
        }

        public void TestCommsMenu()
        {

            bool fDone = false;
            while (!fDone)
            {
                Console.WriteLine("Communications Test Menu\n");
                Console.WriteLine("  V - Firmware version");
                Console.WriteLine("  Q - Quit");

                char ch = Char.ToUpper(Console.ReadKey().KeyChar);
                switch (ch)
                {
                    case 'V': // Get firmware version
                        Console.WriteLine(command.SendSingleCommand((byte)FWCommands.CMD_VERSION));
                        break;
                    case 'Q': // Test Z-rail
                        fDone = true;
                        break;
                }

                Console.WriteLine("\n");
            }
        }

        public void TestMenu()
        {

            bool fDone = false;
            while (!fDone)
            {
                Console.Clear();
                Console.WriteLine("Test Menu\n");
                Console.WriteLine("  Z - Test Z-rail");
                Console.WriteLine("  L - Test laser");
                Console.WriteLine("  G - Test Galvo");
                Console.WriteLine("  C - Test comms");
                Console.WriteLine("  Q - Quit");

                char ch = Char.ToUpper(Console.ReadKey().KeyChar);
                switch (ch)
                {
                    case 'Z': // Test Z-rail
                        TestZRailMenu();
                        break;

                    case 'C': // Test Comms
                        TestCommsMenu();
                        break;

                    case 'Q': // Quit
                        fDone = true;
                        break;
                }
            }
        }

    }
}
