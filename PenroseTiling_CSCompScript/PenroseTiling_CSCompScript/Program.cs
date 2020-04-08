using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using GH_IO;

namespace PenroseTiling_CSCompScript
{
    class Program
    {
        static void Main(string[] args)
        {
            var pt0 = new Point3d(Point3d.Origin);
            var pt1 = new Point3d(10, 10, 30);

            var line = new Line(pt0, pt1);

            Console.WriteLine(line.GetType().ToString());
        }
    }
}
