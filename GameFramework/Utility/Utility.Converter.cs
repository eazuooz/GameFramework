//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------

using System;
using System.Text;

namespace GameFramework
{
    public static partial class Utility
    {
        public static class Converter
        {
            private const float InchesToCentimeters = 2.54f; // 1 inch = 2.54 cm
            private const float CentimetersToInches = 1f / InchesToCentimeters; // 1 cm = 0.3937 inches

            public static bool IsLittleEndian
            {
                get
                {
                    return BitConverter.IsLittleEndian;
                }
            }

            public static float ScreenDpi
            {
                get;
                set;
            }

            public static float GetCentimetersFromPixels(float pixels)
            {
                if (ScreenDpi <= 0)
                {
                    throw new GameFrameworkException("You must set screen DPI first.");
                }

                return InchesToCentimeters * pixels / ScreenDpi;
            }

            public static float GetPixelsFromCentimeters(float centimeters)
            {
                if (ScreenDpi <= 0)
                {
                    throw new GameFrameworkException("You must set screen DPI first.");
                }

                return CentimetersToInches * centimeters * ScreenDpi;
            }

            public static float GetInchesFromPixels(float pixels)
            {
                if (ScreenDpi <= 0)
                {
                    throw new GameFrameworkException("You must set screen DPI first.");
                }

                return pixels / ScreenDpi;
            }

            public static float GetPixelsFromInches(float inches)
            {
                if (ScreenDpi <= 0)
                {
                    throw new GameFrameworkException("You must set screen DPI first.");
                }

                return inches * ScreenDpi;
            }

            public static byte[] GetBytes(bool value)
            {
                byte[] buffer = new byte[1];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(bool value, byte[] buffer)
            {
                GetBytes(value, buffer, 0);
            }

            public static void GetBytes(bool value, byte[] buffer, int startIndex)
            {
                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0 || startIndex + 1 > buffer.Length)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                buffer[startIndex] = value ? (byte)1 : (byte)0;
            }

            public static bool GetBoolean(byte[] value)
            {
                return BitConverter.ToBoolean(value, 0);
            }

            public static bool GetBoolean(byte[] value, int startIndex)
            {
                return BitConverter.ToBoolean(value, startIndex);
            }

            public static byte[] GetBytes(char value)
            {
                byte[] buffer = new byte[2];
                GetBytes((short)value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(char value, byte[] buffer)
            {
                GetBytes((short)value, buffer, 0);
            }

            public static void GetBytes(char value, byte[] buffer, int startIndex)
            {
                GetBytes((short)value, buffer, startIndex);
            }

            public static char GetChar(byte[] value)
            {
                return BitConverter.ToChar(value, 0);
            }

            public static char GetChar(byte[] value, int startIndex)
            {
                return BitConverter.ToChar(value, startIndex);
            }

            public static byte[] GetBytes(short value)
            {
                byte[] buffer = new byte[2];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(short value, byte[] buffer)
            {
                GetBytes(value, buffer, 0);
            }

            public static unsafe void GetBytes(short value, byte[] buffer, int startIndex)
            {
                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0 || startIndex + 2 > buffer.Length)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                fixed (byte* valueRef = buffer)
                {
                    *(short*)(valueRef + startIndex) = value;
                }
            }

            public static short GetInt16(byte[] value)
            {
                return BitConverter.ToInt16(value, 0);
            }

            public static short GetInt16(byte[] value, int startIndex)
            {
                return BitConverter.ToInt16(value, startIndex);
            }

            public static byte[] GetBytes(ushort value)
            {
                byte[] buffer = new byte[2];
                GetBytes((short)value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(ushort value, byte[] buffer)
            {
                GetBytes((short)value, buffer, 0);
            }

            public static void GetBytes(ushort value, byte[] buffer, int startIndex)
            {
                GetBytes((short)value, buffer, startIndex);
            }

            public static ushort GetUInt16(byte[] value)
            {
                return BitConverter.ToUInt16(value, 0);
            }

            public static ushort GetUInt16(byte[] value, int startIndex)
            {
                return BitConverter.ToUInt16(value, startIndex);
            }

            public static byte[] GetBytes(int value)
            {
                byte[] buffer = new byte[4];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(int value, byte[] buffer)
            {
                GetBytes(value, buffer, 0);
            }

            public static unsafe void GetBytes(int value, byte[] buffer, int startIndex)
            {
                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0 || startIndex + 4 > buffer.Length)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                fixed (byte* valueRef = buffer)
                {
                    *(int*)(valueRef + startIndex) = value;
                }
            }

            public static int GetInt32(byte[] value)
            {
                return BitConverter.ToInt32(value, 0);
            }

            public static int GetInt32(byte[] value, int startIndex)
            {
                return BitConverter.ToInt32(value, startIndex);
            }

            public static byte[] GetBytes(uint value)
            {
                byte[] buffer = new byte[4];
                GetBytes((int)value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(uint value, byte[] buffer)
            {
                GetBytes((int)value, buffer, 0);
            }

            public static void GetBytes(uint value, byte[] buffer, int startIndex)
            {
                GetBytes((int)value, buffer, startIndex);
            }

            public static uint GetUInt32(byte[] value)
            {
                return BitConverter.ToUInt32(value, 0);
            }

            public static uint GetUInt32(byte[] value, int startIndex)
            {
                return BitConverter.ToUInt32(value, startIndex);
            }

            public static byte[] GetBytes(long value)
            {
                byte[] buffer = new byte[8];
                GetBytes(value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(long value, byte[] buffer)
            {
                GetBytes(value, buffer, 0);
            }

            public static unsafe void GetBytes(long value, byte[] buffer, int startIndex)
            {
                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0 || startIndex + 8 > buffer.Length)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                fixed (byte* valueRef = buffer)
                {
                    *(long*)(valueRef + startIndex) = value;
                }
            }

            public static long GetInt64(byte[] value)
            {
                return BitConverter.ToInt64(value, 0);
            }

            public static long GetInt64(byte[] value, int startIndex)
            {
                return BitConverter.ToInt64(value, startIndex);
            }

            public static byte[] GetBytes(ulong value)
            {
                byte[] buffer = new byte[8];
                GetBytes((long)value, buffer, 0);
                return buffer;
            }

            public static void GetBytes(ulong value, byte[] buffer)
            {
                GetBytes((long)value, buffer, 0);
            }

            public static void GetBytes(ulong value, byte[] buffer, int startIndex)
            {
                GetBytes((long)value, buffer, startIndex);
            }

            public static ulong GetUInt64(byte[] value)
            {
                return BitConverter.ToUInt64(value, 0);
            }

            public static ulong GetUInt64(byte[] value, int startIndex)
            {
                return BitConverter.ToUInt64(value, startIndex);
            }

            public static unsafe byte[] GetBytes(float value)
            {
                byte[] buffer = new byte[4];
                GetBytes(*(int*)&value, buffer, 0);
                return buffer;
            }

            public static unsafe void GetBytes(float value, byte[] buffer)
            {
                GetBytes(*(int*)&value, buffer, 0);
            }

            public static unsafe void GetBytes(float value, byte[] buffer, int startIndex)
            {
                GetBytes(*(int*)&value, buffer, startIndex);
            }

            public static float GetSingle(byte[] value)
            {
                return BitConverter.ToSingle(value, 0);
            }

            public static float GetSingle(byte[] value, int startIndex)
            {
                return BitConverter.ToSingle(value, startIndex);
            }

            public static unsafe byte[] GetBytes(double value)
            {
                byte[] buffer = new byte[8];
                GetBytes(*(long*)&value, buffer, 0);
                return buffer;
            }

            public static unsafe void GetBytes(double value, byte[] buffer)
            {
                GetBytes(*(long*)&value, buffer, 0);
            }

            public static unsafe void GetBytes(double value, byte[] buffer, int startIndex)
            {
                GetBytes(*(long*)&value, buffer, startIndex);
            }

            public static double GetDouble(byte[] value)
            {
                return BitConverter.ToDouble(value, 0);
            }

            public static double GetDouble(byte[] value, int startIndex)
            {
                return BitConverter.ToDouble(value, startIndex);
            }

            public static byte[] GetBytes(string value)
            {
                return GetBytes(value, Encoding.UTF8);
            }

            public static int GetBytes(string value, byte[] buffer)
            {
                return GetBytes(value, Encoding.UTF8, buffer, 0);
            }

            public static int GetBytes(string value, byte[] buffer, int startIndex)
            {
                return GetBytes(value, Encoding.UTF8, buffer, startIndex);
            }

            public static byte[] GetBytes(string value, Encoding encoding)
            {
                if (value == null)
                {
                    throw new GameFrameworkException("Value is invalid.");
                }

                if (encoding == null)
                {
                    throw new GameFrameworkException("Encoding is invalid.");
                }

                return encoding.GetBytes(value);
            }

            public static int GetBytes(string value, Encoding encoding, byte[] buffer)
            {
                return GetBytes(value, encoding, buffer, 0);
            }

            public static int GetBytes(string value, Encoding encoding, byte[] buffer, int startIndex)
            {
                if (value == null)
                {
                    throw new GameFrameworkException("Value is invalid.");
                }

                if (encoding == null)
                {
                    throw new GameFrameworkException("Encoding is invalid.");
                }

                return encoding.GetBytes(value, 0, value.Length, buffer, startIndex);
            }

            public static string GetString(byte[] value)
            {
                return GetString(value, Encoding.UTF8);
            }

            public static string GetString(byte[] value, Encoding encoding)
            {
                if (value == null)
                {
                    throw new GameFrameworkException("Value is invalid.");
                }

                if (encoding == null)
                {
                    throw new GameFrameworkException("Encoding is invalid.");
                }

                return encoding.GetString(value);
            }

            public static string GetString(byte[] value, int startIndex, int length)
            {
                return GetString(value, startIndex, length, Encoding.UTF8);
            }

            public static string GetString(byte[] value, int startIndex, int length, Encoding encoding)
            {
                if (value == null)
                {
                    throw new GameFrameworkException("Value is invalid.");
                }

                if (encoding == null)
                {
                    throw new GameFrameworkException("Encoding is invalid.");
                }

                return encoding.GetString(value, startIndex, length);
            }
        }
    }
}
