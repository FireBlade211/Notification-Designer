using System.IO;
using Windows.Data.Xml.Dom;

namespace Notification_Designer.Helpers
{
    public static class XmlDocumentHelper
    {
        /// <summary>
        /// Gets a pretty-printed (indented with newlines) XML string.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The pretty XML string.</returns>
        public static string GetPrettyXml(this XmlDocument doc)
        {
            var dotNetDoc = new System.Xml.XmlDocument();
            dotNetDoc.LoadXml(doc.GetXml());

            // Pretty-print it
            var stringWriter = new StringWriter();
            var xmlWriterSettings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = System.Xml.NewLineHandling.Replace,
                OmitXmlDeclaration = false
            };

            using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                dotNetDoc.Save(xmlWriter);
            }

            string prettyXml = stringWriter.ToString();

            return prettyXml;
        }
    }
}
