using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Huffman.Algorithm
{
    public static class Huffman
    {
        // dla optymalizacji wydajności dane będą czytane blokami
        private const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Zwraca drzewo z algorytmu Huffmana dla podanego ciągu bajtów oraz tablicę symboli dla poszczególnych wartości
        /// </summary>
        public static Node GetTree(Stream stream, out Node[] leaves)
        {
            if (!stream.CanSeek)
                throw new InvalidDataException("Stream should be able to seek to compress it");
            stream.Seek(0, SeekOrigin.Begin);
            // utworzenie i inicjalizacja płaskiej tablicy liści
            leaves = new Node[byte.MaxValue + 1];
            for (var i = 0; i <= byte.MaxValue; i++)
                leaves[i] = new Node((byte)i);

            // odczytanie pliku
            byte[] buffer = new byte[BUFFER_SIZE];
            int nBytesRead = 0;
            do
            {
                nBytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < nBytesRead; i++)
                        leaves[buffer[i]].Add(); // zwiększenie licznika użycia
            }
            while (nBytesRead > 0);

            // zbudowanie drzewa i zwrócenie root-a
            return leaves.BuildTree();
        }


        // private members
        private static Node BuildTree(this Node[] nodes)
        {
            if (nodes?.Length == 0)
                return null;
            // odrzucenie pustych i sortowanie pozostałych
            var sorted_nodes = nodes.Where(n => n.Frequency > 0).OrderBy(n => n.Frequency).ToList();
            while (sorted_nodes.Count > 1)
            {
                var left = sorted_nodes[0];
                var right = sorted_nodes[1];
                sorted_nodes.RemoveRange(0, 2);
                var newNode = new Node(left, right);
                // ustalenie pozycji do wstawienia nowego węzła
                var next_node = sorted_nodes.FirstOrDefault(n => n.Frequency > newNode.Frequency);
                if (next_node == null)
                    // jesli miał największą częstotliwość z dotychczasowych
                    sorted_nodes.Add(newNode);
                else
                    // przed węzłem o większej częstotliwości, wyszukiwanie od końca
                    sorted_nodes.Insert(sorted_nodes.LastIndexOf(next_node), newNode);
            }
            // gdy został tylko jeden węzeł
            return sorted_nodes[0];
        }

    }
}
