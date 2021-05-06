using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace PoE_Trade_Bot.Utilities
{
    public static class Extensions
    {
        private const char CR = '\r';
        private const char LF = '\n';
        private const char NULL = (char)0;

        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        /// <summary>
        /// Returns the number of lines in the given <paramref name="stream"/>.
        /// </summary>
        [DebuggerStepThrough]
        public static long CountLines(this Stream stream, Encoding encoding = default)
        {
            var lineCount = 0L;
            var byteBuffer = new byte[1024 * 1024];
            var detectedEOL = NULL;
            var currentChar = NULL;
            int bytesRead;

            if (encoding is null || Equals(encoding, Encoding.ASCII) || Equals(encoding, Encoding.UTF8))
            {
                while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    for (var i = 0; i < bytesRead; i++)
                    {
                        currentChar = (char)byteBuffer[i];

                        if (detectedEOL != NULL)
                        {
                            if (currentChar == detectedEOL)
                            {
                                lineCount++;
                            }
                        }
                        else if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }
            else
            {
                var charBuffer = new char[byteBuffer.Length];

                while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    var charCount = encoding.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0);

                    for (var i = 0; i < charCount; i++)
                    {
                        currentChar = charBuffer[i];

                        if (detectedEOL != NULL)
                        {
                            if (currentChar == detectedEOL)
                            {
                                lineCount++;
                            }
                        }
                        else if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }

            if (currentChar != LF && currentChar != CR && currentChar != NULL)
            {
                lineCount++;
            }

            return lineCount;
        }

    }
}
