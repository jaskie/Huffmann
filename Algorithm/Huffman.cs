using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Huffman.Algorithm
{
    static class Huffman
    {
        // dla optymalizacji wydajności dane będą czytane blokami
        private const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Zwraca drzewo z algorytmu Huffmana dla podanego ciągu bajtów    
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static (Node, Node[]) GetTree(Stream stream)
        {
            if (!stream.CanSeek)
                throw new InvalidDataException("Stream should be able to seek to compress it");
            stream.Seek(0, SeekOrigin.Begin);
            // utworzenie i inicjalizacja płaskiej tablicy węzłów
            Node[] leafNodes = new Node[byte.MaxValue + 1];
            for (var i = 0; i <= byte.MaxValue; i++)
                leafNodes[i] = new Node((byte)i);

            // odczytanie pliku
            byte[] buffer = new byte[BUFFER_SIZE];
            int nBytesRead = 0;
            do
            {
                nBytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < nBytesRead; i++)
                        leafNodes[buffer[i]].Add(); // zwiększenie licznika użycia
            }
            while (nBytesRead > 0);

            // zbudowanie drzewa
            return (leafNodes.BuildTree(), leafNodes);
        }


        // private members
        private static Node BuildTree(this IEnumerable<Node> nodes)
        {
            // odrzucenie pustych i sortowanie
            var sorted_nodes = nodes.Where(n => n.Frequency > 0).OrderBy(n => n.Frequency).ToList();
            while (sorted_nodes.Count > 1)
            {
                var left = sorted_nodes[0];
                var right = sorted_nodes[1];
                sorted_nodes.RemoveRange(0, 2);
                var newNode = new Node(left, right);
                left.Parent = newNode;
                right.Parent = newNode;
                var next_node = sorted_nodes.FirstOrDefault(n => n.Frequency > newNode.Frequency);
                if (next_node == null)
                    sorted_nodes.Add(newNode);
                else
                    sorted_nodes.Insert(sorted_nodes.LastIndexOf(next_node), newNode);
            }
            return sorted_nodes.FirstOrDefault();
        }

    }
}
