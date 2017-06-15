using System;
using System.IO;

namespace Huffman.IO
{
    public class BitStream: IDisposable
    {
        private readonly Stream _stream;
        private int _bitIndex; // bit index to write to
        private byte _currentByte; // byte to write being composed

        public BitStream(Stream stream)
        {
            _stream = stream;
        }

        public void Write(bool bit)
        {
            if (bit)
                _currentByte |= (byte)(1 << _bitIndex);
            if (++_bitIndex > 7)
            {
                _stream.WriteByte(_currentByte);
                _bitIndex = 0;
                _currentByte = 0;
            }
        }

        public void Write(long value)
        {
            if (_bitIndex == 0)
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
            if (_bitIndex == 0)
                _stream.WriteByte(value);
            else
            {
                var size = sizeof(byte) * 8;
                for (var i = 0; i < size; i++)
                    Write(((value >>= 1) & 1) != 0);
            }
        }

        public bool Read(out bool result)
        {
            if (_bitIndex == 0)
            {
                var i = _stream.ReadByte();
                if (-1 == i)  //EOF
                {
                    result = false;
                    return false;
                }
                else
                    _currentByte = (byte)i;
            }
            result = (_currentByte & (1 << _bitIndex)) > 0;
            if (++_bitIndex > 7)
                _bitIndex = 0;
            return true;
        }

        public bool Read(out byte result)
        {
            result = 0;
            if (_bitIndex == 0)
            {
                int i = _stream.ReadByte();
                if (-1 == i) //EOF
                    return false;
                result = (byte)i;
                return true;
            }
            var size = sizeof(byte) * 8;
            for (var i = 0; i < size; i++)
            {
                if (Read(out bool bit) && bit)
                    result |= (byte)(1 << i);
            }
            return true;
        }

        public bool Read(out long result)
        {
            result = 0;
            if (_bitIndex == 0)
            {
                byte[] buffer = new byte[sizeof(long)];
                int nread = _stream.Read(buffer, 0, sizeof(long));
                if (sizeof(long) > nread)
                    return false; // EOF
                result = BitConverter.ToInt64(buffer, 0);
                return true;
            }
            var size = sizeof(byte) * 8;
            for (var i = 0; i < size; i++)
            {
                if (Read(out bool bit) && bit)
                    result |= (byte)(1 << i);
            }
            return true;
        }

        private bool _isDisposed = false; // To detect redundant calls
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (_bitIndex > 0 && _stream.CanWrite)
                    _stream.WriteByte(_currentByte);
                _stream.Dispose();
            }
        }

    }
}
