using System.Threading.Tasks;
using System.Xml;

namespace Elmo.Viewer.Utilities
{
    internal static class XmlWriterExtensions
    {
        public static Task WriteStartElementAsync(this XmlWriter writer, string localName)
        {
            return writer.WriteStartElementAsync(null, localName, null);
        }

        public static Task WriteAttributeStringAsync(this XmlWriter writer, string localName, string value)
        {
            return writer.WriteAttributeStringAsync(null, localName, null, value);
        }

        public static Task WriteDocTypeAsync(this XmlWriter writer, string name)
        {
            return writer.WriteDocTypeAsync(name, null, null, null);
        }

        public static Task WriteElementStringAsync(this XmlWriter writer, string localName, string value)
        {
            return writer.WriteElementStringAsync(null, localName, null, value);
        }
    }
}
