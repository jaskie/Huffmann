using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Huffman
{
    public class Codec : IDisposable
    {

        public enum CodecFunction
        {
            Encode,
            Decode
        }

        public static void Encode(string inputFilename, string outputFileName)
        {
            using (var encoder = new Codec(inputFilename, outputFileName, CodecFunction.Encode))
            {
                encoder.WriteHeader();
                // utworzenie i zapisanie drzewa do strumienia
                var leaves = encoder.WriteTree();
                // zapisanie symboli dla zawartości pliku
                encoder.WriteContent(leaves);
            }
        }

        public static void Decode(string inputFileName, string outputFileName)
        {
            using (var decoder = new Codec(inputFileName, outputFileName, CodecFunction.Decode))
            {
                long size = decoder.ReadHeader();
                var tree = decoder.ReadTree();
                decoder.ReadContent(tree, size);
            }
        }


        private readonly FileStream Uncompressed;
        private readonly IO.BitStream Encoded;
        private const int BUFFER_SIZE = 4096;

        public Codec(string inputFilename, string outputFilename, CodecFunction function)
        {
            Uncompressed = function == CodecFunction.Encode ? File.OpenRead(inputFilename) : File.Open(outputFilename, FileMode.Create);
            Encoded = new IO.BitStream(function == CodecFunction.Encode ? File.Open(outputFilename, FileMode.Create) : File.OpenRead(inputFilename));
        }

        /// <summary>
        /// Zapisuje nagłówek pliku - długość pliku (8 bajtów)
        /// </summary>
        public void WriteHeader()
        {
            Encoded.Write(Uncompressed.Length);
        }

        /// <summary>
        /// Odczytuje nagłówek pliku (długość)
        /// </summary>
        /// <returns></returns>
        public long ReadHeader()
        {
            if (!Encoded.Read(out long result))
                throw new Exception("Nie udało się odczytanie nagłówka pliku");
            return result;
        }

        /// <summary>
        /// tworzy drzewo i zapisuje je do pliku
        /// </summary>
        public Algorithm.Node[] WriteTree()
        {
            var tree = Algorithm.Huffman.GetTree(Uncompressed, out Algorithm.Node[] leaves);
            WriteNode(tree);
            return leaves;
        }

        /// <summary>
        /// odczyt drzewa
        /// </summary>
        public Algorithm.Node ReadTree()
        {
            return ReadNode();
        }

        /// <summary>
        /// zapisuje zawartość pliku w postaci symboli
        /// </summary>
        public void WriteContent(Algorithm.Node[] leaves)
        {
            Uncompressed.Seek(0, SeekOrigin.Begin);
            // odczytanie pliku
            byte[] buffer = new byte[BUFFER_SIZE];
            int nBytesRead = 0;
            do
            {
                nBytesRead = Uncompressed.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < nBytesRead; i++)
                    WriteSymbol(leaves[buffer[i]]);
            }
            while (nBytesRead > 0);
        }

        /// <summary>
        /// Odczytuje symbole z zapisuje do pliku
        /// </summary>
        public void ReadContent(Algorithm.Node tree, long size)
        {
            for (int i = 0; i < size; i++)
                Uncompressed.WriteByte(ReadByte(tree));
        }

        #region Utilities
        /// <summary>
        /// rekurencyjnie zapisuje węzeł i całą jego zawartość
        /// </summary>
        private void WriteNode(Algorithm.Node node)
        {
            if (node.Key.HasValue)
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

        /// <summary>
        /// odczytuje węzeł i jego zawartość
        /// </summary>
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
            }
            return newNode;
        }

        /// <summary>
        /// zapisuje pojedynczy bit symbolu
        /// </summary>
        private void WriteSymbol(Algorithm.Node symbol)
        {
            if (symbol.Parent?.Parent != null)
                WriteSymbol(symbol.Parent);
            Encoded.Write(symbol.Parent.Left != symbol);
        }

        /// <summary>
        /// odczytuje bajt począwszy od wskazanego węzła drzewa
        /// </summary>
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
            if (!_isDisposed)
            {
                _isDisposed = true;
                Encoded.Dispose();
                Uncompressed.Dispose();
            }
        }
        #endregion
    }
}

