using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Notmat.Xliff12
{
    /// <summary>
    /// Root element for XLIFF 1.2 documents
    /// </summary>
    [XmlRoot("xliff", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
    public class XliffDocument
    {
        /// <summary>
        /// XLIFF version
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; } = "1.2";

        /// <summary>
        /// XML namespace
        /// </summary>
        [XmlAttribute("xmlns")]
        public string Xmlns { get; set; } = "urn:oasis:names:tc:xliff:document:1.2";

        /// <summary>
        /// Collection of file elements
        /// </summary>
        [XmlElement("file")]
        public List<XliffFile> Files { get; set; } = new List<XliffFile>();
    }

    /// <summary>
    /// Represents a file element in XLIFF 1.2
    /// </summary>
    public class XliffFile
    {
        /// <summary>
        /// Original file path
        /// </summary>
        [XmlAttribute("original")]
        public string Original { get; set; } = string.Empty;

        /// <summary>
        /// Source language code
        /// </summary>
        [XmlAttribute("source-language")]
        public string SourceLanguage { get; set; } = string.Empty;

        /// <summary>
        /// Target language code
        /// </summary>
        [XmlAttribute("target-language")]
        public string? TargetLanguage { get; set; }

        /// <summary>
        /// Data type (e.g., "xml", "plaintext")
        /// </summary>
        [XmlAttribute("datatype")]
        public string DataType { get; set; } = "xml";

        /// <summary>
        /// File header information
        /// </summary>
        [XmlElement("header")]
        public XliffHeader? Header { get; set; }

        /// <summary>
        /// File body containing translation units
        /// </summary>
        [XmlElement("body")]
        public XliffBody Body { get; set; } = new XliffBody();
    }

    /// <summary>
    /// Represents the header section of an XLIFF 1.2 file
    /// </summary>
    public class XliffHeader
    {
        /// <summary>
        /// Tool information
        /// </summary>
        [XmlElement("tool")]
        public XliffTool? Tool { get; set; }

        /// <summary>
        /// Notes about the file
        /// </summary>
        [XmlElement("note")]
        public List<XliffNote> Notes { get; set; } = new List<XliffNote>();
    }

    /// <summary>
    /// Represents tool information in XLIFF 1.2 header
    /// </summary>
    public class XliffTool
    {
        /// <summary>
        /// Tool identifier
        /// </summary>
        [XmlAttribute("tool-id")]
        public string ToolId { get; set; } = "notmat";

        /// <summary>
        /// Tool name
        /// </summary>
        [XmlAttribute("tool-name")]
        public string ToolName { get; set; } = "Notmat XLIFF Library";

        /// <summary>
        /// Tool version
        /// </summary>
        [XmlAttribute("tool-version")]
        public string ToolVersion { get; set; } = "1.0.0";
    }

    /// <summary>
    /// Represents the body section containing translation units
    /// </summary>
    public class XliffBody
    {
        /// <summary>
        /// Collection of translation units
        /// </summary>
        [XmlElement("trans-unit")]
        public List<TranslationUnit> TranslationUnits { get; set; } = new List<TranslationUnit>();

        /// <summary>
        /// Collection of groups
        /// </summary>
        [XmlElement("group")]
        public List<XliffGroup> Groups { get; set; } = new List<XliffGroup>();
    }

    /// <summary>
    /// Represents a group of translation units
    /// </summary>
    public class XliffGroup
    {
        /// <summary>
        /// Group identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Group name
        /// </summary>
        [XmlAttribute("resname")]
        public string? ResourceName { get; set; }

        /// <summary>
        /// Translation units in this group
        /// </summary>
        [XmlElement("trans-unit")]
        public List<TranslationUnit> TranslationUnits { get; set; } = new List<TranslationUnit>();

        /// <summary>
        /// Nested groups
        /// </summary>
        [XmlElement("group")]
        public List<XliffGroup> Groups { get; set; } = new List<XliffGroup>();
    }

    /// <summary>
    /// Represents a translation unit in XLIFF 1.2
    /// </summary>
    public class TranslationUnit
    {
        /// <summary>
        /// Unique identifier for the translation unit
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Resource name
        /// </summary>
        [XmlAttribute("resname")]
        public string? ResourceName { get; set; }

        /// <summary>
        /// Translation state
        /// </summary>
        [XmlAttribute("approved")]
        public string? Approved { get; set; }

        /// <summary>
        /// Translation state (needs-translation, translated, etc.)
        /// </summary>
        [XmlIgnore]
        public TranslationState State { get; set; } = TranslationState.New;

        /// <summary>
        /// XML serialization property for State
        /// </summary>
        [XmlAttribute("state")]
        public string? StateString
        {
            get => State == TranslationState.New ? null : State.ToString().ToLowerInvariant().Replace("_", "-");
            set => State = ParseTranslationState(value);
        }

        /// <summary>
        /// Source text
        /// </summary>
        [XmlElement("source")]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Target text (translation)
        /// </summary>
        [XmlElement("target")]
        public string? Target { get; set; }

        /// <summary>
        /// Notes about this translation unit
        /// </summary>
        [XmlElement("note")]
        public List<XliffNote> Notes { get; set; } = new List<XliffNote>();

        private static TranslationState ParseTranslationState(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return TranslationState.New;

            return value.ToLowerInvariant() switch
            {
                "new" => TranslationState.New,
                "needs-translation" => TranslationState.NeedsTranslation,
                "needs-review-translation" => TranslationState.NeedsReviewTranslation,
                "translated" => TranslationState.Translated,
                "final" => TranslationState.Final,
                "needs-adaptation" => TranslationState.NeedsAdaptation,
                "needs-l10n" => TranslationState.NeedsL10n,
                "needs-review-adaptation" => TranslationState.NeedsReviewAdaptation,
                "needs-review-l10n" => TranslationState.NeedsReviewL10n,
                "signed-off" => TranslationState.SignedOff,
                _ => TranslationState.New
            };
        }
    }

    /// <summary>
    /// Represents a note in XLIFF 1.2
    /// </summary>
    public class XliffNote
    {
        /// <summary>
        /// Note priority
        /// </summary>
        [XmlAttribute("priority")]
        public string? Priority { get; set; }

        /// <summary>
        /// From attribute
        /// </summary>
        [XmlAttribute("from")]
        public string? From { get; set; }

        /// <summary>
        /// Note content
        /// </summary>
        [XmlText]
        public string Content { get; set; } = string.Empty;
    }
}