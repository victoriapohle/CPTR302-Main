using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCoder
{
    class GCode
    {
        string fileName = "";
        System.IO.StreamReader inputfile = null;
        public GCode(string fileName)
        {
            this.fileName = fileName;

            inputfile = new System.IO.StreamReader(fileName);

        }

        public bool GetNextLine(ref string cmd, ref float X, ref float Y, ref float Z, ref float E)
        {
            string line = "";

            while (line == "") // Skip blank lines
            {
                line = inputfile.ReadLine();
                if (line != null) line = line.Trim();
            }

            if (line == null)  // end of file reached, close file and return false
            {
                inputfile.Close();
                return false; 
            }

            int ndx = 0;
            char param;
            string paramValue;

            X = 0; Y = 0; Z = 0; E = 0;
            cmd = "";

            if (line == "") return false;

            // Skip initial white space
            while (ndx < line.Length && Char.IsWhiteSpace(line[ndx]))
                ndx++;

            // Get command
            while (ndx < line.Length && !Char.IsWhiteSpace(line[ndx]))
                cmd += line[ndx++];

            if (cmd.ToUpper() == "G1")  // Get parameters
            {
                while (ndx < line.Length)
                {
                    // Skip white space
                    while (ndx < line.Length && Char.IsWhiteSpace(line[ndx]))
                        ndx++;

                    // Get param identifier
                    param = line[ndx++];
                    paramValue = "";

                    // Get parameter value
                    while (ndx < line.Length && !Char.IsWhiteSpace(line[ndx]))
                        paramValue += line[ndx++];

                    if (Char.ToUpper(param) == 'X')
                        X = float.Parse(paramValue);
                    else if (Char.ToUpper(param) == 'Y')
                        Y = float.Parse(paramValue);
                    else if (Char.ToUpper(param) == 'Z')
                        Z = float.Parse(paramValue);
                    else if (Char.ToUpper(param) == 'E')
                        E = float.Parse(paramValue);
                }
            }

            return true;
        }
    }
}
