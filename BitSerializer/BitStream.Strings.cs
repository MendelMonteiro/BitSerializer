﻿using System;
using System.Diagnostics;
using System.Text;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        public const byte StringLengthMax = byte.MaxValue;

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public string ReadString(Encoding encoding)
        {
            ushort byteCount = Math.Min(ReadUShort(), (ushort)(StringLengthMax * 4));

            if (byteCount == 0)
                return string.Empty;

            byte* buffer = stackalloc byte[byteCount];
            ReadMemory(buffer, byteCount);
            return new string((sbyte*)buffer, 0, byteCount, encoding);
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public int ReadString(char[] destination, int offset, Encoding encoding)
        {
            if ((uint)offset >= destination.Length)
                throw new ArgumentOutOfRangeException("Offset exceeds array size.");


            fixed (char* ptr = &destination[offset])
            {
                return ReadStringInternal(ptr, destination.Length - offset, encoding);
            }
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public int ReadString(char* ptr, int charLength, Encoding encoding)
        {
            return ReadStringInternal(ptr, charLength, encoding);
        }


        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. Includes the bytelength as an uint16.
        /// </summary>
        public BitStreamer WriteString(string str, Encoding encoding)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            fixed (char* ptr = str)
            {
                WriteString(ptr, str.Length, encoding);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. Includes the bytelength as an uint16.
        /// </summary>
        public BitStreamer WriteString(char[] str, Encoding encoding)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            fixed (char* ptr = str)
            {
                WriteString(ptr, str.Length, encoding);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. Includes the bytelength as an uint16.
        /// </summary>
        public BitStreamer WriteString(char* ptr, int charCount, Encoding encoding)
        {
            if (charCount > StringLengthMax)
                throw new ArgumentOutOfRangeException("String length exceeds maximum allowed size of " + StringLengthMax);

            int byteLength = encoding.GetByteCount(ptr, charCount);
            WriteUShort((ushort)byteLength);

            byte* bytes = stackalloc byte[byteLength];
            encoding.GetBytes(ptr, charCount, bytes, byteLength);
            WriteMemory(bytes, byteLength);

            return this;
        }

        private int ReadStringInternal(char* str, int charLength, Encoding encoding)
        {
            if (charLength < 1)
                throw new ArgumentOutOfRangeException(nameof(charLength), "charLength must be at least 1.");

            ushort byteCount = Math.Min(ReadUShort(), (ushort)(StringLengthMax * 4));

            if (byteCount == 0)
                return 0;

            byte* buffer = stackalloc byte[byteCount];
            int charCount = Math.Min(encoding.GetCharCount(buffer, byteCount), charLength);

            ReadMemory(buffer, byteCount);
            encoding.GetChars(buffer, byteCount, str, charCount);

            return charCount;
        }
    }
}
