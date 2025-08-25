using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Notmat
{
    /// <summary>
    /// Provides functionality to load XLIFF files of different versions
    /// </summary>
    public class XliffLoader
    {
        /// <summary>
        /// Loads an XLIFF file and automatically detects the version
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>A tuple containing the XLIFF version and the document object</returns>
        /// <exception cref="ArgumentNullException">Thrown when filePath is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when the XLIFF version is unsupported</exception>
        public static (XliffVersion Version, object Document) LoadXliff(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"XLIFF file not found: {filePath}");

            var version = DetectXliffVersion(filePath);
            
            return version switch
            {
                XliffVersion.Version12 => (version, LoadXliff12(filePath)),
                XliffVersion.Version20 => (version, LoadXliff20(filePath)),
                _ => throw new InvalidOperationException($"Unsupported XLIFF version detected in file: {filePath}")
            };
        }

        /// <summary>
        /// Asynchronously loads an XLIFF file and automatically detects the version
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>A tuple containing the XLIFF version and the document object</returns>
        public static async Task<(XliffVersion Version, object Document)> LoadXliffAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"XLIFF file not found: {filePath}");

            var version = await DetectXliffVersionAsync(filePath);
            
            return version switch
            {
                XliffVersion.Version12 => (version, await LoadXliff12Async(filePath)),
                XliffVersion.Version20 => (version, await LoadXliff20Async(filePath)),
                _ => throw new InvalidOperationException($"Unsupported XLIFF version detected in file: {filePath}")
            };
        }

        /// <summary>
        /// Loads an XLIFF 1.2 file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>XLIFF 1.2 document</returns>
        public static Xliff12.XliffDocument LoadXliff12(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Xliff12.XliffDocument));
            using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var document = (Xliff12.XliffDocument?)serializer.Deserialize(reader);
            return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 1.2 document");
        }

        /// <summary>
        /// Asynchronously loads an XLIFF 1.2 file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>XLIFF 1.2 document</returns>
        public static async Task<Xliff12.XliffDocument> LoadXliff12Async(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Xliff12.XliffDocument));
            using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            
            return await Task.Run(() =>
            {
                var document = (Xliff12.XliffDocument?)serializer.Deserialize(reader);
                return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 1.2 document");
            });
        }

        /// <summary>
        /// Loads an XLIFF 2.0 file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>XLIFF 2.0 document</returns>
        public static Xliff20.XliffDocument LoadXliff20(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Xliff20.XliffDocument));
            using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var document = (Xliff20.XliffDocument?)serializer.Deserialize(reader);
            return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 2.0 document");
        }

        /// <summary>
        /// Asynchronously loads an XLIFF 2.0 file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>XLIFF 2.0 document</returns>
        public static async Task<Xliff20.XliffDocument> LoadXliff20Async(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Xliff20.XliffDocument));
            using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            
            return await Task.Run(() =>
            {
                var document = (Xliff20.XliffDocument?)serializer.Deserialize(reader);
                return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 2.0 document");
            });
        }

        /// <summary>
        /// Detects the XLIFF version from the file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>Detected XLIFF version</returns>
        /// <exception cref="InvalidOperationException">Thrown when version cannot be detected</exception>
        public static XliffVersion DetectXliffVersion(string filePath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                var root = doc.DocumentElement;
                if (root?.Name != "xliff")
                    throw new InvalidOperationException("Invalid XLIFF file: root element is not 'xliff'");

                var version = root.GetAttribute("version");
                return version switch
                {
                    "1.2" => XliffVersion.Version12,
                    "2.0" => XliffVersion.Version20,
                    _ => throw new InvalidOperationException($"Unsupported XLIFF version: {version}")
                };
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"Failed to detect XLIFF version from file: {filePath}", ex);
            }
        }

        /// <summary>
        /// Asynchronously detects the XLIFF version from the file
        /// </summary>
        /// <param name="filePath">Path to the XLIFF file</param>
        /// <returns>Detected XLIFF version</returns>
        public static async Task<XliffVersion> DetectXliffVersionAsync(string filePath)
        {
            return await Task.Run(() => DetectXliffVersion(filePath));
        }

        /// <summary>
        /// Loads XLIFF from string content
        /// </summary>
        /// <param name="xliffContent">XLIFF XML content</param>
        /// <returns>A tuple containing the XLIFF version and the document object</returns>
        public static (XliffVersion Version, object Document) LoadXliffFromString(string xliffContent)
        {
            if (string.IsNullOrEmpty(xliffContent))
                throw new ArgumentNullException(nameof(xliffContent));

            var version = DetectXliffVersionFromString(xliffContent);
            
            return version switch
            {
                XliffVersion.Version12 => (version, LoadXliff12FromString(xliffContent)),
                XliffVersion.Version20 => (version, LoadXliff20FromString(xliffContent)),
                _ => throw new InvalidOperationException("Unsupported XLIFF version detected in content")
            };
        }

        /// <summary>
        /// Loads XLIFF 1.2 from string content
        /// </summary>
        /// <param name="xliffContent">XLIFF XML content</param>
        /// <returns>XLIFF 1.2 document</returns>
        public static Xliff12.XliffDocument LoadXliff12FromString(string xliffContent)
        {
            var serializer = new XmlSerializer(typeof(Xliff12.XliffDocument));
            using var reader = new StringReader(xliffContent);
            var document = (Xliff12.XliffDocument?)serializer.Deserialize(reader);
            return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 1.2 document");
        }

        /// <summary>
        /// Loads XLIFF 2.0 from string content
        /// </summary>
        /// <param name="xliffContent">XLIFF XML content</param>
        /// <returns>XLIFF 2.0 document</returns>
        public static Xliff20.XliffDocument LoadXliff20FromString(string xliffContent)
        {
            var serializer = new XmlSerializer(typeof(Xliff20.XliffDocument));
            using var reader = new StringReader(xliffContent);
            var document = (Xliff20.XliffDocument?)serializer.Deserialize(reader);
            return document ?? throw new InvalidOperationException("Failed to deserialize XLIFF 2.0 document");
        }

        /// <summary>
        /// Detects the XLIFF version from string content
        /// </summary>
        /// <param name="xliffContent">XLIFF XML content</param>
        /// <returns>Detected XLIFF version</returns>
        public static XliffVersion DetectXliffVersionFromString(string xliffContent)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xliffContent);

                var root = doc.DocumentElement;
                if (root?.Name != "xliff")
                    throw new InvalidOperationException("Invalid XLIFF content: root element is not 'xliff'");

                var version = root.GetAttribute("version");
                return version switch
                {
                    "1.2" => XliffVersion.Version12,
                    "2.0" => XliffVersion.Version20,
                    _ => throw new InvalidOperationException($"Unsupported XLIFF version: {version}")
                };
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException("Failed to detect XLIFF version from content", ex);
            }
        }
    }
}