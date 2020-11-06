using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleSN.Core
{
    public class DataGenerator
    {
        public IEnumerable<Point> Generate(int amountOfsources, double maxDistanceForPointFromSource, int amountOfPoints, Point startPoint, Point endPoint)
        {
            Random rnd = new Random();
            var sources = new Point[amountOfsources];
            for (int i = 0; i < amountOfsources; i++)
            {
                sources[i] = new Point(rnd.Next(startPoint.X, endPoint.X), rnd.Next(startPoint.Y, endPoint.Y));
            }

            for (int i = 0; i < amountOfPoints; i++)
            {
                var randomSource = sources[rnd.Next(0, sources.Length)];
                Size distanceFromSource = new Size(width: (int)(rnd.NextDouble() * maxDistanceForPointFromSource), height: (int)(rnd.NextDouble() * maxDistanceForPointFromSource));
                yield return new Point(x: randomSource.X + distanceFromSource.Width, y: randomSource.Y + distanceFromSource.Height);
            }
        }
    }

}
