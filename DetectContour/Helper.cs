using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DetectContour
{
    public static class Helper
    {
        public static XDocument SerializeToXDoc<T>(this T source)
        {
            var result = new XDocument();
            var serializer = new XmlSerializer(source.GetType());
            using (var writer = result.CreateWriter())
            {
                serializer.Serialize(writer, source);
            }
            return result;
        }

        public static List<C2DPoint> ConvertPointFToC2DPointDummy(IEnumerable<System.Drawing.PointF> input)
        {
            return input.Select(p => new C2DPoint {x = p.X, y = p.Y}).ToList();
        }
    }
}
