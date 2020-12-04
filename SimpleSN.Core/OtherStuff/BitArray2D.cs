using SimpleSN.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSN.GUI
{
    public sealed class BitArray2D
    {
        private BitArray _array;
        private int _dimension1;
        private int _dimension2;
        public string Name { get; set; }

        public Size Size => new Size(_dimension1, _dimension2);

        public BitArray2D(System.Drawing.Size size) : this(size.Width, size.Height)
        {

        }

        public BitArray2D(int dimension1, int dimension2)
        {
            _dimension1 = dimension1 > 0 ? dimension1 : throw new ArgumentOutOfRangeException(nameof(dimension1), dimension1, string.Empty);
            _dimension2 = dimension2 > 0 ? dimension2 : throw new ArgumentOutOfRangeException(nameof(dimension2), dimension2, string.Empty);
            _array = new BitArray(dimension1 * dimension2);
        }

        public BitArray2D(IEnumerable<double> vector, Size size) : this(size.Width, size.Height)
        {
            int pixel = 0;
            foreach (var el in vector)
            {
                Set(pixel % size.Width, pixel / size.Height, el > 0);
                ++pixel;
            }
        }

        public BitArray2D(string pbmFilename)
        {
            Regex findWhiteSpaces = new Regex(@"\s");
            Name = System.IO.Path.GetFileName(pbmFilename);
            var rawLines = System.IO.File.ReadAllLines(pbmFilename);
            var wasTypeReaded = false;
            var size = System.Drawing.Size.Empty;
            int currentPoint = 0;
            foreach (var line in rawLines.Select(d => d.Trim()))
            {
                if (line.StartsWith('#')) continue;
                if (line.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                {
                    var number = Convert.ToInt32(line[1].ToString());
                    if (number != 1) throw new InvalidOperationException($"The format of bitmap should be P1 but is P{number}!");
                    wasTypeReaded = true;
                    continue;
                }
                if (Char.IsDigit(line[0]) && size == Size.Empty)
                {
                    string[] sizeAsStrings = findWhiteSpaces.Split(line);
                    if (sizeAsStrings.Length != 2) throw new InvalidOperationException($"Something else than two nubmers is in size line! The line: {line}.");
                    size = new Size(Convert.ToInt32(sizeAsStrings[0]), Convert.ToInt32(sizeAsStrings[1]));
                    _dimension1 = size.Width;
                    _dimension2 = size.Height;
                    _array = new BitArray(size.GetArea());
                }
                else if (Char.IsDigit(line[0]))
                {
                    var bits = findWhiteSpaces.Split(line);
                    foreach (var bit in bits)
                    {
                        if (currentPoint > size.GetArea()) throw new ArgumentOutOfRangeException($"Size specified in file is smaller than data in file!");
                        _array.Set(currentPoint++, Convert.ToInt32(bit) == 1 ? true : false);
                    }
                }
                else if (Char.IsLetter(line[0]))
                {
                    throw new NotImplementedException($"The data line started with letter. I don't know what to do! Line: {line}.");
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int x = 0; x < _dimension1; x++)
            {
                for (int y = 0; y < _dimension2; y++)
                {
                    builder.Append(_array.Get((x * _dimension1) + y) ? 'X' : ' ');
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public bool Get(int w) => _array.Get(w);
        public bool Get(int x, int y) { CheckBounds(x, y); return _array[y * _dimension1 + x]; }
        public bool Set(int x, int y, bool val) { CheckBounds(x, y); return _array[y * _dimension1 + x] = val; }
        public bool this[int x, int y] { get { return Get(x, y); } set { Set(x, y, value); } }
        public IEnumerable<bool> GetVector()
        {
            foreach (object foo in _array)
            {
                yield return Convert.ToBoolean(foo);
            }
        }

        private void CheckBounds(int x, int y)
        {
            if (x < 0 || x >= _dimension1)
            {
                throw new IndexOutOfRangeException();
            }
            if (y < 0 || y >= _dimension2)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
