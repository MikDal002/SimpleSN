using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleSN.Core
{
    public class VectorsFactory
    {
        internal static IEnumerable<double[]> GetFromLab1()
        {
            yield return new[] { 0.4, 0.3, 0.1, 0.7 };
            yield return new[] { 0.8, 0.8, 0.8, 0.7 };
            yield return new[] { 0.2, 0.5, 0.2, 0.1 };
            yield return new[] { 0.5, 0.3, 0.2, 0.7 };
        }

        internal static IEnumerable<double[]> FromFile(string fileName, string separator = ",", bool skipFirstLine = true)
        {
            using var openStream = System.IO.File.OpenRead(fileName);
            using var reader = new StreamReader(openStream);
            var readedLine = "";
            while (readedLine != null)
            {
                readedLine = reader.ReadLine();
                if (skipFirstLine)
                {
                    skipFirstLine = false;
                    continue;
                }
                if (readedLine == null) break;

                var splittedLine = readedLine.Split(separator);
                yield return splittedLine.Select(d => d.Replace('.', ',')).Select(d => double.Parse(d)).ToArray();
            }
        }
    }

}
