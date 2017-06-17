using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Huffman.Algorithm
{
    [DebuggerDisplay("{_key}:{_frequency}")]
    public class Node
    {
        private readonly byte? _key;
        private int _frequency;

        /// <summary>
        /// Konstruktor dla liści
        /// </summary>
        /// <param name="key"></param>
        public Node(byte key)
        {
            _key = key;
        }

        /// <summary>
        /// Konstruktor dla węzłów
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public Node(Node left, Node right)
        {
            Left = left;
            Right = right;
            left.Parent = this;
            right.Parent = this;
            _frequency = left.Frequency + right.Frequency;
        }

        public Node Parent { get; set; }
        public Node Left { get; }
        public Node Right { get; }

        public byte? Key { get => _key; }
        public int Frequency { get => _frequency; }

        public void Add()
        {
            _frequency++;
        }
    }
}
