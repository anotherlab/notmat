using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Notmat.Converters
{
    /// <summary>
    /// Provides functionality to convert XLIFF files to .resx format
    /// </summary>
    public class XliffToResxConverter
    {
        /// <summary>
        /// Converts an XLIFF 1.2 document to .resx format
        /// </summary>
        /// <param name="xliffDocument">XLIFF 1.2 document to convert</param>
        /// <param name="outputPath">Output path for the .resx file</param>
        /// <param name="useTargetAsValue">If true, uses target text as value; if false, uses source text</param>
        /// <param name="includeUntranslated">If true, includes entries without target text</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        public static void ConvertFromXliff12(Xliff12.XliffDocument xliffDocument, string outputPath, bool useTargetAsValue = true, bool includeUntranslated = true)
        {
            if (xliffDocument == null)
                throw new ArgumentNullException(nameof(xliffDocument));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

            var entries = new List<ResourceEntry>();

            foreach (var file in xliffDocument.Files)
            {
                // Process translation units from the main body
                foreach (var unit in file.Body.TranslationUnits)
                {
                    AddTranslationUnitToEntries(entries, unit.Id, unit.ResourceName, unit.Source, unit.Target, useTargetAsValue, includeUntranslated);
                }

                // Process translation units from groups
                foreach (var group in file.Body.Groups)
                {
                    ProcessGroup(entries, group, useTargetAsValue, includeUntranslated);
                }
            }

            WriteResxFile(outputPath, entries);
        }

        /// <summary>
        /// Converts an XLIFF 2.0 document to .resx format
        /// </summary>
        /// <param name="xliffDocument">XLIFF 2.0 document to convert</param>
        /// <param name="outputPath">Output path for the .resx file</param>
        /// <param name="useTargetAsValue">If true, uses target text as value; if false, uses source text</param>
        /// <param name="includeUntranslated">If true, includes entries without target text</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        public static void ConvertFromXliff20(Xliff20.XliffDocument xliffDocument, string outputPath, bool useTargetAsValue = true, bool includeUntranslated = true)
        {
            if (xliffDocument == null)
                throw new ArgumentNullException(nameof(xliffDocument));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

            var entries = new List<ResourceEntry>();

            foreach (var file in xliffDocument.Files)
            {
                // Process units directly in the file
                foreach (var unit in file.Units)
                {
                    ProcessUnit20(entries, unit, useTargetAsValue, includeUntranslated);
                }

                // Process units in groups
                foreach (var group in file.Groups)
                {
                    ProcessGroup20(entries, group, useTargetAsValue, includeUntranslated);
                }
            }

            WriteResxFile(outputPath, entries);
        }

        /// <summary>
        /// Converts an XLIFF document to .resx format (auto-detects version)
        /// </summary>
        /// <param name="xliffDocument">XLIFF document to convert</param>
        /// <param name="outputPath">Output path for the .resx file</param>
        /// <param name="useTargetAsValue">If true, uses target text as value; if false, uses source text</param>
        /// <param name="includeUntranslated">If true, includes entries without target text</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        /// <exception cref="ArgumentException">Thrown when document type is not supported</exception>
        public static void ConvertFromXliff(object xliffDocument, string outputPath, bool useTargetAsValue = true, bool includeUntranslated = true)
        {
            if (xliffDocument == null)
                throw new ArgumentNullException(nameof(xliffDocument));

            switch (xliffDocument)
            {
                case Xliff12.XliffDocument doc12:
                    ConvertFromXliff12(doc12, outputPath, useTargetAsValue, includeUntranslated);
                    break;
                case Xliff20.XliffDocument doc20:
                    ConvertFromXliff20(doc20, outputPath, useTargetAsValue, includeUntranslated);
                    break;
                default:
                    throw new ArgumentException($"Unsupported document type: {xliffDocument.GetType()}");
            }
        }

        /// <summary>
        /// Converts XLIFF file to .resx format
        /// </summary>
        /// <param name="xliffFilePath">Path to the XLIFF file</param>
        /// <param name="outputPath">Output path for the .resx file</param>
        /// <param name="useTargetAsValue">If true, uses target text as value; if false, uses source text</param>
        /// <param name="includeUntranslated">If true, includes entries without target text</param>
        public static void ConvertFile(string xliffFilePath, string outputPath, bool useTargetAsValue = true, bool includeUntranslated = true)
        {
            if (string.IsNullOrEmpty(xliffFilePath))
                throw new ArgumentNullException(nameof(xliffFilePath));
            if (!File.Exists(xliffFilePath))
                throw new FileNotFoundException($"XLIFF file not found: {xliffFilePath}");

            var (version, document) = XliffLoader.LoadXliff(xliffFilePath);
            ConvertFromXliff(document, outputPath, useTargetAsValue, includeUntranslated);
        }

        private static void ProcessGroup(List<ResourceEntry> entries, Xliff12.XliffGroup group, bool useTargetAsValue, bool includeUntranslated)
        {
            foreach (var unit in group.TranslationUnits)
            {
                AddTranslationUnitToEntries(entries, unit.Id, unit.ResourceName, unit.Source, unit.Target, useTargetAsValue, includeUntranslated);
            }

            foreach (var nestedGroup in group.Groups)
            {
                ProcessGroup(entries, nestedGroup, useTargetAsValue, includeUntranslated);
            }
        }

        private static void ProcessGroup20(List<ResourceEntry> entries, Xliff20.XliffGroup group, bool useTargetAsValue, bool includeUntranslated)
        {
            foreach (var unit in group.Units)
            {
                ProcessUnit20(entries, unit, useTargetAsValue, includeUntranslated);
            }

            foreach (var nestedGroup in group.Groups)
            {
                ProcessGroup20(entries, nestedGroup, useTargetAsValue, includeUntranslated);
            }
        }

        private static void ProcessUnit20(List<ResourceEntry> entries, Xliff20.TranslationUnit unit, bool useTargetAsValue, bool includeUntranslated)
        {
            foreach (var segment in unit.Segments)
            {
                var key = unit.Name ?? unit.Id;
                if (unit.Segments.Count > 1)
                {
                    key = $"{key}_{segment.Id}";
                }

                AddTranslationUnitToEntries(entries, key, unit.Name, segment.Source, segment.Target, useTargetAsValue, includeUntranslated);
            }
        }

        private static void AddTranslationUnitToEntries(List<ResourceEntry> entries, string id, string? resourceName, string source, string? target, bool useTargetAsValue, bool includeUntranslated)
        {
            var hasTarget = !string.IsNullOrEmpty(target);
            
            if (!hasTarget && !includeUntranslated)
                return;

            var key = resourceName ?? id;
            var value = useTargetAsValue && hasTarget ? target! : source;

            entries.Add(new ResourceEntry(key, value));
        }

        private static void WriteResxFile(string outputPath, List<ResourceEntry> entries)
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("root",
                    // Standard .resx headers
                    new XElement("resheader",
                        new XAttribute("name", "resmimetype"),
                        new XElement("value", "text/microsoft-resx")),
                    new XElement("resheader",
                        new XAttribute("name", "version"),
                        new XElement("value", "2.0")),
                    new XElement("resheader",
                        new XAttribute("name", "reader"),
                        new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                    new XElement("resheader",
                        new XAttribute("name", "writer"),
                        new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),

                    // Data entries
                    entries.Select(entry =>
                        new XElement("data",
                            new XAttribute("name", entry.Key),
                            new XAttribute(XNamespace.Get("http://www.w3.org/XML/1998/namespace") + "space", "preserve"),
                            new XElement("value", entry.Value)))
                ));

            doc.Save(outputPath);
        }

        /// <summary>
        /// Represents a resource entry for .resx file generation
        /// </summary>
        private record ResourceEntry(string Key, string Value);
    }
}