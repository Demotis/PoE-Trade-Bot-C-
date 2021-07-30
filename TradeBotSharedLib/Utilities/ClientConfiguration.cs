using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TradeBotSharedLib.Utilities
{
    public static class ClientConfiguration
    {
        public static string ToBase64String(this Bitmap bmp, ImageFormat imageFormat)
        {
            if (bmp == null) return string.Empty;
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);
            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();
            memoryStream.Close();
            string base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;
            return base64String;
        }

        public static Bitmap Base64StringToBitmap(this string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String)) return null;
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;
            Bitmap bmpReturn = (Bitmap)Image.FromStream(memoryStream);
            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;
            return bmpReturn;
        }

        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string resolutionNormal, T objectToWrite, bool append = false) where T : new()
        {
            string filePath = $"ClientConfig\\{resolutionNormal}.conf";
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            TextWriter writer = null;
            try
            {

                var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string resolutionNormal) where T : new()
        {
            string filePath = $"ClientConfig\\{resolutionNormal}.conf";

            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
            }
            catch
            {
                return default(T);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
