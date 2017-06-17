using System;
using System.IO;
using System.Text;

namespace Huffman
{
    class Program
    {
        static void Main(string[] args)
        {
            // dodaje obsługę UTF-8 w konsoli
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string inputFileName = null;
            string outputFileName = null;
            bool decoding = false;
            foreach (var parameter in args)
            {
                if (null == inputFileName && null == outputFileName)
                {
                    if (parameter.ToLowerInvariant().Equals("-d"))
                        decoding = true;
                    else
                        inputFileName = parameter;
                }
                else
                if (null == outputFileName)
                {
                    outputFileName = parameter;
                }
                else
                    break; // mamy wszystkie parametry                
            }
            if (!string.IsNullOrEmpty(inputFileName) && !string.IsNullOrEmpty(outputFileName) && File.Exists(inputFileName))
            {
                if (decoding)
                    Codec.Decode(inputFileName, outputFileName);
                else
                    Codec.Encode(inputFileName, outputFileName);
            }
            else
                Console.WriteLine("Użycie: Huffmann [-d] <plik wejściowy> <plik wynikowy>");
        }
    }
}