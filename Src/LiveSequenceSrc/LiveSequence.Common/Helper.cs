using System.Collections.Generic;
using System.IO;
using System.Text;
namespace LiveSequence.Common
{
    public sealed class Helper
    {
        public static string RemoveExtension(string filename, string ext)
        {
            if (filename.EndsWith(ext))
            {
                filename = filename.Remove(
                    filename.Length - ext.Length, ext.Length);
            }

            return filename;
        }

        public static string RemoveAnyExtension(string filename)
        {
            int length = filename.Length - filename.LastIndexOf('.');
            return RemoveExtension(filename, filename.Substring(filename.LastIndexOf('.'), length));
        }

        public static string RemoveInvalidCharsFromFileName(string fileName)
        {
            char[] fileNameChars = fileName.ToCharArray();
            StringBuilder result = new StringBuilder(fileName.Length);

            List<char> invalidChars = new List<char>();
            invalidChars.AddRange(Path.GetInvalidFileNameChars());

            foreach (var nameChar in fileNameChars)
            {
                if (invalidChars.Contains(nameChar))
                {
                    result.Append('_');
                }
                else
                {
                    result.Append(nameChar);
                }
            }

            return result.ToString();
        }
    }
}