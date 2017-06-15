using System;
using System.IO;
using System.Text;

namespace Huffman
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFileName = null;
            string outputFileName = null;
            bool decoding = false;
            foreach (var par in args)
            {
                if (null == inputFileName && null == outputFileName)
                {
                    if (par.ToLowerInvariant().Equals("-d"))
                        decoding = true;
                    else
                        inputFileName = par;
                }
                else
                if (null == outputFileName)
                {
                    outputFileName = par;
                }
                else
                    break; // mamy wszystkie parametry                
            }
            if (!string.IsNullOrEmpty(inputFileName) && !string.IsNullOrEmpty(outputFileName) && File.Exists(inputFileName))
                if (decoding)
                { }
                  
            else
                Compressor.Compress(inputFileName, outputFileName);
            
        }
    }
}