using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml;
using System.Xml.Linq;

namespace Notmat.Converters
{
    /// <summary>
    /// Provides functionality to convert .resx files to XLIFF format
    /// </summary>
    public class ResxToXliffConverter
    {
        /// <summary>
        /// Converts a .resx file to XLIFF 1.2 format
        /// </summary>
        /// <param name="resxFilePath">Path to the source .resx file</param>
        /// <param name="sourceLanguage">Source language code (e.g., "en-US")</param>
        /// <param name="targetLanguage">Target language code (optional, e.g., "fr-FR")</param>
        /// <returns>XLIFF 1.2 document</returns>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the .resx file doesn't exist</exception>
        public static Xliff12.XliffDocument ConvertToXliff12(string resxFilePath, string sourceLanguage, string? targetLanguage = null)
        {
            if (string.IsNullOrEmpty(resxFilePath))
                throw new ArgumentNullException(nameof(resxFilePath));
            if (string.IsNullOrEmpty(sourceLanguage))
                throw new ArgumentNullException(nameof(sourceLanguage));
            if (!File.Exists(resxFilePath))
                throw new FileNotFoundException($"Resx file not found: {resxFilePath}");

            var resxEntries = ReadResxFile(resxFilePath);
            
            var xliffDoc = new Xliff12.XliffDocument();
            var xliffFile = new Xliff12.XliffFile
            {
                Original = Path.GetFileName(resxFilePath),
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage,
                DataType = "resx",
                Header = new Xliff12.XliffHeader
                {
                    Tool = new Xliff12.XliffTool()
                }
            };

            foreach (var entry in resxEntries)
            {
                var translationUnit = new Xliff12.TranslationUnit
                {
                    Id = entry.Key,
                    ResourceName = entry.Key,
                    Source = entry.Value,
                    State = TranslationState.New
                };

                xliffFile.Body.TranslationUnits.Add(translationUnit);
            }

            xliffDoc.Files.Add(xliffFile);
            return xliffDoc;
        }

        /// <summary>
        /// Converts a .resx file to XLIFF 2.0 format
        /// </summary>
        /// <param name="resxFilePath">Path to the source .resx file</param>
        /// <param name="sourceLanguage">Source language code (e.g., "en-US")</param>
        /// <param name="targetLanguage">Target language code (optional, e.g., "fr-FR")</param>
        /// <returns>XLIFF 2.0 document</returns>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the .resx file doesn't exist</exception>
        public static Xliff20.XliffDocument ConvertToXliff20(string resxFilePath, string sourceLanguage, string? targetLanguage = null)
        {
            if (string.IsNullOrEmpty(resxFilePath))
                throw new ArgumentNullException(nameof(resxFilePath));
            if (string.IsNullOrEmpty(sourceLanguage))
                throw new ArgumentNullException(nameof(sourceLanguage));
            if (!File.Exists(resxFilePath))
                throw new FileNotFoundException($"Resx file not found: {resxFilePath}");

            var resxEntries = ReadResxFile(resxFilePath);
            
            var xliffDoc = new Xliff20.XliffDocument
            {
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage
            };

            var xliffFile = new Xliff20.XliffFile
            {
                Id = Path.GetFileNameWithoutExtension(resxFilePath),
                Original = Path.GetFileName(resxFilePath),
                Header = new Xliff20.XliffHeader
                {
                    Tool = new Xliff20.XliffTool()
                }
            };

            foreach (var entry in resxEntries)
            {
                var unit = new Xliff20.TranslationUnit
                {
                    Id = entry.Key,
                    Name = entry.Key
                };

                var segment = new Xliff20.Segment
                {
                    Id = "1",
                    Source = entry.Value,
                    State = TranslationState.New
                };

                unit.Segments.Add(segment);
                xliffFile.Units.Add(unit);
            }

            xliffDoc.Files.Add(xliffFile);
            return xliffDoc;
        }

        /// <summary>
        /// Converts a .resx file to the specified XLIFF version
        /// </summary>
        /// <param name="resxFilePath">Path to the source .resx file</param>
        /// <param name="sourceLanguage">Source language code</param>
        /// <param name="xliffVersion">Target XLIFF version</param>
        /// <param name="targetLanguage">Target language code (optional)</param>
        /// <returns>XLIFF document object</returns>
        public static object ConvertToXliff(string resxFilePath, string sourceLanguage, XliffVersion xliffVersion, string? targetLanguage = null)
        {
            return xliffVersion switch
            {
                XliffVersion.Version12 => ConvertToXliff12(resxFilePath, sourceLanguage, targetLanguage),
                XliffVersion.Version20 => ConvertToXliff20(resxFilePath, sourceLanguage, targetLanguage),
                _ => throw new ArgumentException($"Unsupported XLIFF version: {xliffVersion}")
            };
        }

        /// <summary>
        /// Reads key-value pairs from a .resx file
        /// </summary>
        /// <param name="resxFilePath">Path to the .resx file</param>
        /// <returns>Dictionary of resource keys and values</returns>
        private static Dictionary<string, string> ReadResxFile(string resxFilePath)
        {
            var entries = new Dictionary<string, string>();

            try
            {
                // Use XDocument for better control over XML parsing
                var doc = XDocument.Load(resxFilePath);
                var dataElements = doc.Descendants("data");

                foreach (var element in dataElements)
                {
                    var nameAttr = element.Attribute("name");
                    var valueElement = element.Element("value");

                    if (nameAttr != null && valueElement != null)
                    {
                        var key = nameAttr.Value;
                        var value = valueElement.Value;

                        // Only include string resources, skip other types
                        var typeAttr = element.Attribute("type");
                        if (typeAttr == null || typeAttr.Value.Contains("System.String"))
                        {
                            entries[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read .resx file: {resxFilePath}", ex);
            }

            return entries;
        }

        /// <summary>
        /// Validates that a file is a valid .resx file
        /// </summary>
        /// <param name="resxFilePath">Path to the file to validate</param>
        /// <returns>True if the file is a valid .resx file</returns>
        public static bool IsValidResxFile(string resxFilePath)
        {
            if (string.IsNullOrEmpty(resxFilePath) || !File.Exists(resxFilePath))
                return false;

            try
            {
                var doc = XDocument.Load(resxFilePath);
                return doc.Root?.Name.LocalName == "root" && 
                       doc.Descendants("data").Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets resource count from a .resx file
        /// </summary>
        /// <param name="resxFilePath">Path to the .resx file</param>
        /// <returns>Number of string resources in the file</returns>
        public static int GetResourceCount(string resxFilePath)
        {
            if (!IsValidResxFile(resxFilePath))
                return 0;

            try
            {
                return ReadResxFile(resxFilePath).Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}