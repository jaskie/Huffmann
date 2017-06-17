using System;
using System.IO;

namespace Huffman.IO
{
    /// <summary>
    /// Klasa zapisywania i odczytywania bitowego strumienia danych
    /// </summary>
    public class BitStream: IDisposable
    {
        private readonly Stream _stream; // strumień, na którym są przeprowadzane operacje odczytu albo zapisu
        private int _bitIndex; // aktualny numer bitu
        private byte _currentByte; // aktualny bajt, który jest odczytywany lub będzie zapisany
        private bool _isDisposed = false; // na wypadek powtórzenia wywołania Dispose

        public BitStream(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// zapis pojedynczego bitu
        /// </summary>
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

        /// <summary>
        /// Zapis bajtu, jeśli zerowy indeks bitu, to bezpośrednio do strumienia
        /// </summary>
        public void Write(byte value)
        {
            if (_bitIndex == 0)
                _stream.WriteByte(value);
            else
            {
                var size = sizeof(byte) * 8;
                for (var i = 0; i < size; i++)
                    Write((value & (1 << i)) != 0);
            }
        }

        /// <summary>
        /// Zapis Int64, jeśli zerowy indeks bitu, to bezpośrednio do strumienia
        /// </summary>
        public void Write(long value)
        {
            if (_bitIndex == 0)
                _stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
            else
            {
                var size = sizeof(long) * 8;
                for (var i = 0; i < size; i++)
                    Write((value & (1L << i)) != 0);
            }
        }

        /// <summary>
        /// odczyt pojedynczego bitu
        /// </summary>
        /// <param name="result">odczytany bit</param>
        /// <returns>false, jeśli koniec pliku</returns>
        public bool Read(out bool result)
        {
            if (_bitIndex == 0)
            {
                var i = _stream.ReadByte();
                if (i == -1)  //EOF
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

        /// <summary>
        /// odczyt bajtu
        /// </summary>
        /// <param name="result">odczytany bit</param>
        /// <returns>false, jeśli koniec pliku</returns>
        public bool Read(out byte result)
        {
            result = 0;
            if (_bitIndex == 0)
            {
                int i = _stream.ReadByte();
                if (i == -1) //EOF
                    return false;
                result = (byte)i;
                return true;
            }
            var size = sizeof(byte) * 8;
            for (var i = 0; i < size; i++)
            {
                if (Read(out bool bit))
                {
                    if (bit)
                        result |= (byte)(1 << i);
                }
                else return false;
            }
            return true;
        }

        /// <summary>
        /// odczyt Int64
        /// </summary>
        /// <param name="result">odczytany bit</param>
        /// <returns>false, jeśli koniec pliku</returns>
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
                if (Read(out bool bit))
                {
                    if (bit)
                        result |= (byte)(1 << i);
                }
                else return false;
            }
            return true;
        }

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
