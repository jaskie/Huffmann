#undef DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Huffman
{
    public class Codec : IDisposable
    {

        public static void Encode(string inputFilename, string outputFileName)
        {
            using (var encoder = new Codec(inputFilename, outputFileName, true))
            {
                encoder.WriteHeader();
                // write whole tree as the method is recurrent
                encoder.WriteNode(encoder.Tree);
                // write file bytes as symbols
                encoder.WriteContent();
            }
        }

        public static void Decode(string inputFileName, string outputFileName)
        {
            using (var decoder = new Codec(inputFileName, outputFileName, false))
            {
                long size = decoder.ReadHeader();
                decoder.Leaves = new Algorithm.Node[byte.MaxValue + 1];
                decoder.Tree = decoder.ReadNode();
                decoder.ReadContent(size);
            }
        }


        private readonly FileStream Uncompressed;
        private readonly IO.BitStream Encoded;
        private Algorithm.Node Tree;
        private Algorithm.Node[] Leaves;
        private const int BUFFER_SIZE = 4096;

        private Codec(string inputFilename, string outputFilename, bool encode)
        {
            Uncompressed = encode ? File.OpenRead(inputFilename) : File.Open(outputFilename, FileMode.Create);
            Encoded = new IO.BitStream( encode ? File.Open(outputFilename, FileMode.Create) : File.OpenRead(inputFilename));
            if (encode)
                (Tree, Leaves) = Algorithm.Huffman.GetTree(Uncompressed);
        }


        private void WriteHeader()
        {
            Encoded.Write(Uncompressed.Length);
        }

        private long ReadHeader()
        {
            if (!Encoded.Read(out long result))
                throw new Exception("Nie udało się odczytanie nagłówka pliku");
            return result;
        }
        

        private void WriteNode(Algorithm.Node node)
        {
            if (node.Left == null && node.Right == null)
            {
                Encoded.Write(false);
                Encoded.Write((byte)node.Key);
            }
            else
            {
                Encoded.Write(true);
                WriteNode(node.Left);
                WriteNode(node.Right);
            }
        }

        private Algorithm.Node ReadNode()
        {
            if (!Encoded.Read(out bool bit))
                throw new Exception("Nie udało się czytanie drzewa Huffmanna");
            Algorithm.Node newNode;
            if (bit)
                newNode = new Algorithm.Node(ReadNode(), ReadNode());
            else
            {
                if (!Encoded.Read(out byte value))
                    throw new Exception("Nie udało się czytanie drzewa Huffmanna");
                newNode = new Algorithm.Node(value);
                Leaves[value] = newNode;
            }
            return newNode;
        }

        private void WriteContent()
        {
            Uncompressed.Seek(0, SeekOrigin.Begin);
            // odczytanie pliku
            byte[] buffer = new byte[BUFFER_SIZE];
            int nBytesRead = 0;
            do
            {
                nBytesRead = Uncompressed.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < nBytesRead; i++)
                    WriteByte(buffer[i]);
            }
            while (nBytesRead > 0);
        }

        private void ReadContent(long size)
        {
            for (int i = 0; i < size; i++)
                Uncompressed.WriteByte(ReadByte(Tree));
        }

        private void WriteByte(byte value)
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
            Encoded.Write(symbol.Parent.Left != symbol);
            Debug.Write(symbol.Parent.Left != symbol ? 1 : 0);
        }

        private byte ReadByte(Algorithm.Node node)
        {
            if (!Encoded.Read(out bool bit))
                throw new Exception("Nie udało się czytanie zawartości pliku");
            var next = bit ? node.Right : node.Left;
            if (next == null)
                throw new Exception("Archiwum jest uszkodzone");
            if (next.Left == null)
                return (byte)next.Key;
            return ReadByte(next);
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            if(!_isDisposed)
            {
                _isDisposed = true;
                Encoded.Dispose();
                Uncompressed.Dispose();
            }
        }
    }
}
