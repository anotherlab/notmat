using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Notmat
{
    /// <summary>
    /// Provides functionality to save XLIFF files of different versions
    /// </summary>
    public class XliffSaver
    {
        /// <summary>
        /// Saves an XLIFF 1.2 document to a file
        /// </summary>
        /// <param name="document">XLIFF 1.2 document to save</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <exception cref="ArgumentNullException">Thrown when document or filePath is null</exception>
        public static void SaveXliff12(Xliff12.XliffDocument document, string filePath, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var serializer = new XmlSerializer(typeof(Xliff12.XliffDocument));
            var settings = new XmlWriterSettings
            {
                Indent = formatting,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var writer = XmlWriter.Create(filePath, settings);
            
            // Add namespaces
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "urn:oasis:names:tc:xliff:document:1.2");
            
            serializer.Serialize(writer, document, namespaces);
        }

        /// <summary>
        /// Asynchronously saves an XLIFF 1.2 document to a file
        /// </summary>
        /// <param name="document">XLIFF 1.2 document to save</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        public static async Task SaveXliff12Async(Xliff12.XliffDocument document, string filePath, bool formatting = true)
        {
            await Task.Run(() => SaveXliff12(document, filePath, formatting));
        }

        /// <summary>
        /// Saves an XLIFF 2.0 document to a file
        /// </summary>
        /// <param name="document">XLIFF 2.0 document to save</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <exception cref="ArgumentNullException">Thrown when document or filePath is null</exception>
        public static void SaveXliff20(Xliff20.XliffDocument document, string filePath, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var serializer = new XmlSerializer(typeof(Xliff20.XliffDocument));
            var settings = new XmlWriterSettings
            {
                Indent = formatting,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var writer = XmlWriter.Create(filePath, settings);
            
            // Add namespaces
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "urn:oasis:names:tc:xliff:document:2.0");
            
            serializer.Serialize(writer, document, namespaces);
        }

        /// <summary>
        /// Asynchronously saves an XLIFF 2.0 document to a file
        /// </summary>
        /// <param name="document">XLIFF 2.0 document to save</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        public static async Task SaveXliff20Async(Xliff20.XliffDocument document, string filePath, bool formatting = true)
        {
            await Task.Run(() => SaveXliff20(document, filePath, formatting));
        }

        /// <summary>
        /// Saves XLIFF document to string based on version
        /// </summary>
        /// <param name="document">XLIFF document to save</param>
        /// <param name="version">XLIFF version</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <returns>XLIFF XML content as string</returns>
        /// <exception cref="ArgumentNullException">Thrown when document is null</exception>
        /// <exception cref="ArgumentException">Thrown when version and document type don't match</exception>
        public static string SaveXliffToString(object document, XliffVersion version, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return version switch
            {
                XliffVersion.Version12 when document is Xliff12.XliffDocument doc12 => SaveXliff12ToString(doc12, formatting),
                XliffVersion.Version20 when document is Xliff20.XliffDocument doc20 => SaveXliff20ToString(doc20, formatting),
                _ => throw new ArgumentException($"Document type does not match specified version {version}")
            };
        }

        /// <summary>
        /// Saves an XLIFF 1.2 document to string
        /// </summary>
        /// <param name="document">XLIFF 1.2 document to save</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <returns>XLIFF XML content as string</returns>
        public static string SaveXliff12ToString(Xliff12.XliffDocument document, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var serializer = new XmlSerializer(typeof(Xliff12.XliffDocument));
            var settings = new XmlWriterSettings
            {
                Indent = formatting,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter, settings);
            
            // Add namespaces
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "urn:oasis:names:tc:xliff:document:1.2");
            
            serializer.Serialize(writer, document, namespaces);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Saves an XLIFF 2.0 document to string
        /// </summary>
        /// <param name="document">XLIFF 2.0 document to save</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <returns>XLIFF XML content as string</returns>
        public static string SaveXliff20ToString(Xliff20.XliffDocument document, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var serializer = new XmlSerializer(typeof(Xliff20.XliffDocument));
            var settings = new XmlWriterSettings
            {
                Indent = formatting,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter, settings);
            
            // Add namespaces
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "urn:oasis:names:tc:xliff:document:2.0");
            
            serializer.Serialize(writer, document, namespaces);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Automatically saves an XLIFF document based on its type
        /// </summary>
        /// <param name="document">XLIFF document to save (1.2 or 2.0)</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        /// <exception cref="ArgumentNullException">Thrown when document or filePath is null</exception>
        /// <exception cref="ArgumentException">Thrown when document type is not supported</exception>
        public static void SaveXliff(object document, string filePath, bool formatting = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            switch (document)
            {
                case Xliff12.XliffDocument doc12:
                    SaveXliff12(doc12, filePath, formatting);
                    break;
                case Xliff20.XliffDocument doc20:
                    SaveXliff20(doc20, filePath, formatting);
                    break;
                default:
                    throw new ArgumentException($"Unsupported document type: {document.GetType()}");
            }
        }

        /// <summary>
        /// Asynchronously saves an XLIFF document based on its type
        /// </summary>
        /// <param name="document">XLIFF document to save (1.2 or 2.0)</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="formatting">Whether to format the XML output</param>
        public static async Task SaveXliffAsync(object document, string filePath, bool formatting = true)
        {
            await Task.Run(() => SaveXliff(document, filePath, formatting));
        }
    }
}