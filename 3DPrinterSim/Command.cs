using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandTypes;
using Hardware;

namespace CommandHandler
{
    public class Command
    {
        const byte HEADER_SIZE = 4;

        List<byte> curCommand = new List<byte>();
        PrinterControl printer;
        long cmdNum = 0;

        public Command(PrinterControl printer)
        {
            this.printer = printer;
        }

        public long Count() { return curCommand.Count; }
        public void Clear()
        {
            curCommand.Clear();
        }

        public void Add(byte value)
        {
            curCommand.Add(value);
        }
        public void Add(float value)
        {
            byte[] vOut = BitConverter.GetBytes(value);
        
            for (int i = 0; i < vOut.Length; i++)
                curCommand.Add(vOut[i]);
        }

        public string SendCommand()
        {
            byte[] value = new byte[HEADER_SIZE];
            int bytesRetrieved;
            string retString = "";

            // Calculate checksum
            short checkSum = 0;
            for (int i = 0; i < curCommand.Count(); i++)
                checkSum += curCommand[i];

            curCommand[2] = (byte) checkSum;   // Stuff checksum into buffer at reserved spot
            curCommand[3] = (byte) (checkSum >> 8);

            do
            {
                // Write the header
                printer.WriteSerialToFirmware(curCommand.ToArray(), HEADER_SIZE);
                // Get header back
                while ((bytesRetrieved = printer.ReadSerialFromFirmware(value, HEADER_SIZE)) == 0)
                    ;

                if (value[0] == curCommand[0] && value[1] == curCommand[1] && value[2] == curCommand[2] && value[3] == curCommand[3])
                {
                    // Send ACK
                    value[0] = 0xA5;
                    printer.WriteSerialToFirmware(value, 1);

                    // Send command payload
                    while (printer.WriteSerialToFirmware(curCommand.Skip(HEADER_SIZE).ToArray(), curCommand.Count()-HEADER_SIZE) == 0)
                        ;

                    retString = "";
                    // Wait for response byte
                    do
                    {
                        bytesRetrieved = printer.ReadSerialFromFirmware(value, 1);
                    } while (bytesRetrieved == 0);

                    retString += (char)value[0];  // Add returned value to return string

                    // Add rest of response string until null byte read
                    while (value[0] != 0)
                    {
                        bytesRetrieved = printer.ReadSerialFromFirmware(value, 1);
                        if (bytesRetrieved != 0 && value[0] != 0) retString += (char)value[0];
                    }

                    if (curCommand[0] == FWCommands.CMD_VERSION) break; // Return version info

                    //                    if (retString != "SUCCESS") Console.WriteLine("Error {0}: " + retString, cmdNum);

                }
                else  // error - send NACK
                {
                    // Send NACK
                    value[0] = 0xFF;
                    printer.WriteSerialToFirmware(value, 1);
                    retString = "NAK";
                }

            } while (retString != "SUCCESS");

            Clear();
            cmdNum++;

            return retString;
        }

        public string SendSingleCommand(byte Command)
        {
            Clear();
            Add(Command);
            Add(1);
            Add(0); // Placeholder for checksum
            Add(0); // Placeholder for checksum
            Add(0); // dummy parameter
            return SendCommand();
        }
        public string SendCommandParam1(byte Command, byte param)
        {
            Clear();
            Add(Command);
            Add(1);
            Add(0); // Placeholder for checksum
            Add(0); // Placeholder for checksum
            Add(param);
            return SendCommand();
        }

        public string SendCommandParam1(byte Command, float param)
        {
            Clear();
            Add(Command);
            Add(4);
            Add(0); // Placeholder for checksum
            Add(0); // Placeholder for checksum
            Add(param);
            return SendCommand();
        }
        public string SendCommandParam2(byte Command, float param1, float param2)
        {
            Clear();
            Add(Command);
            Add(8);
            Add(0); // Placeholder for checksum
            Add(0); // Placeholder for checksum
            Add(param1);
            Add(param2);
            return SendCommand();
        }

        public void StartCommandBuffer()
        {
            Clear();
            Add(FWCommands.CMD_BUFFER);
            Add(0);  // Placeholder for length
            Add(0);  // Placeholder for checksum
            Add(0); // Placeholder for checksum
        }

        public string SendCommandBuffer()
        {
            curCommand[1] = (byte) (curCommand.Count - HEADER_SIZE);
            return SendCommand();
        }
    }
}
