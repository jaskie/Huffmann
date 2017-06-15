#undef DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Huffman
{
    public class Compressor : IDisposable
    {

        public static void Compress(string inpytFilename, string outputFileName)
        {
            using (var compressor = new Compressor(inpytFilename, outputFileName))
            {
                compressor.WriteHeader();
                // write whole tree as the method is recurrent
                compressor.WriteNode(compressor.Tree);
                // write file bytes as symbols
                compressor.WriteContent();
            }
        }

        private readonly FileStream Input;
        private readonly IO.BitStream Output;
        private readonly Algorithm.Node Tree;
        private readonly Algorithm.Node[] Leaves;
        private const int BUFFER_SIZE = 4096;

        private Compressor(string inputFilename, string outputFilename)
        {
            Input = File.OpenRead(inputFilename);
            Output = new IO.BitStream(File.Open(outputFilename, FileMode.Create));
            (Tree, Leaves) = Algorithm.Huffman.GetTree(Input);
        }

        private void WriteHeader()
        {
            Output.Write(Input.Length);
        }

        private void WriteNode(Algorithm.Node node)
        {
            if (node.Left == null && node.Right == null)
            {
                Output.Write(false);
                Output.Write((byte)node.Key);
            }
            else
            {
                Output.Write(true);
                WriteNode(node.Left);
                WriteNode(node.Right);
            }
        }

        private void WriteContent()
        {
            Input.Seek(0, SeekOrigin.Begin);
            // odczytanie pliku
            byte[] buffer = new byte[BUFFER_SIZE];
            int nBytesRead = 0;
            do
            {
                nBytesRead = Input.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < nBytesRead; i++)
                    WriteValue(buffer[i]);
            }
            while (nBytesRead > 0);
        }

        private void WriteValue(byte value)
        {
            var node = Leaves[value];
            Debug.Write($"{Encoding.ASCII.GetString(new[] { value })}:");
            WriteSymbol(node);
            Debug.WriteLine(" ");
        }

        private void WriteSymbol(Algorithm.Node symbol)
        {
            if (symbol.Parent?.Parent != null)
                WriteSymbol(symbol.Parent);
            Output.Write(symbol.Parent.Left != symbol);
            Debug.Write(symbol.Parent.Left != symbol ? 1 : 0);
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            if(!_isDisposed)
            {
                _isDisposed = true;
                Output.Dispose();
                Input.Dispose();
            }
        }
    }
}
