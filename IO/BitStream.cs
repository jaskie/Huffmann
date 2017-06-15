using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Huffman.IO
{
    public class BitStream: IDisposable
    {
        private readonly Stream _stream;
        private int _bitIxToWrite; // bit index to write to
        private int _bitIxToRead; // bit index to read from
        private byte _byteToWrite; // byte to write being composed
        private byte _byteToRead;

        public BitStream(Stream stream)
        {
            _stream = stream;
        }

        public void Write(bool bit)
        {
            if (bit)
                _byteToWrite |= (byte)(1 << _bitIxToWrite);
            if (++_bitIxToWrite > 7)
            {
                _stream.WriteByte(_byteToWrite);
                _bitIxToWrite = 0;
                _byteToWrite = 0;
            }
        }

        public void Write(long value)
        {
            if (0 == _bitIxToWrite)
                _stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
            else
            {
                var size = sizeof(long) * 8;
                for (var i = 0; i < size; i++)
                    Write(((value >>= 1) & 1) != 0);
            }
        }

        public void Write(byte value)
        {
            if (0 == _bitIxToWrite)
                _stream.WriteByte(value);
            else
            {
                var size = sizeof(byte) * 8;
                for (var i = 0; i < size; i++)
                    Write(((value >>= 1) & 1) != 0);
            }
        }

        public bool ReadBit()
        {
            return true;
        }

        private bool _isDisposed = false; // To detect redundant calls
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (_bitIxToWrite > 0)
                    _stream.WriteByte(_byteToWrite);
                _stream.Flush();
            }
        }

    }
}
