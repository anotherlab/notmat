using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Notmat.Xliff20
{
    /// <summary>
    /// Root element for XLIFF 2.0 documents
    /// </summary>
    [XmlRoot("xliff", Namespace = "urn:oasis:names:tc:xliff:document:2.0")]
    public class XliffDocument
    {
        /// <summary>
        /// XLIFF version
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; } = "2.0";

        /// <summary>
        /// XML namespace
        /// </summary>
        [XmlAttribute("xmlns")]
        public string Xmlns { get; set; } = "urn:oasis:names:tc:xliff:document:2.0";

        /// <summary>
        /// Source language code
        /// </summary>
        [XmlAttribute("srcLang")]
        public string SourceLanguage { get; set; } = string.Empty;

        /// <summary>
        /// Target language code
        /// </summary>
        [XmlAttribute("trgLang")]
        public string? TargetLanguage { get; set; }

        /// <summary>
        /// Collection of file elements
        /// </summary>
        [XmlElement("file")]
        public List<XliffFile> Files { get; set; } = new List<XliffFile>();
    }

    /// <summary>
    /// Represents a file element in XLIFF 2.0
    /// </summary>
    public class XliffFile
    {
        /// <summary>
        /// File identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Original file path
        /// </summary>
        [XmlAttribute("original")]
        public string? Original { get; set; }

        /// <summary>
        /// File header information
        /// </summary>
        [XmlElement("header")]
        public XliffHeader? Header { get; set; }

        /// <summary>
        /// Collection of units
        /// </summary>
        [XmlElement("unit")]
        public List<TranslationUnit> Units { get; set; } = new List<TranslationUnit>();

        /// <summary>
        /// Collection of groups
        /// </summary>
        [XmlElement("group")]
        public List<XliffGroup> Groups { get; set; } = new List<XliffGroup>();
    }

    /// <summary>
    /// Represents the header section of an XLIFF 2.0 file
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
    /// Represents tool information in XLIFF 2.0 header
    /// </summary>
    public class XliffTool
    {
        /// <summary>
        /// Tool identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = "notmat";

        /// <summary>
        /// Tool name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; } = "Notmat XLIFF Library";

        /// <summary>
        /// Tool version
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; } = "1.0.0";
    }

    /// <summary>
    /// Represents a group of translation units in XLIFF 2.0
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
        [XmlAttribute("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Translation units in this group
        /// </summary>
        [XmlElement("unit")]
        public List<TranslationUnit> Units { get; set; } = new List<TranslationUnit>();

        /// <summary>
        /// Nested groups
        /// </summary>
        [XmlElement("group")]
        public List<XliffGroup> Groups { get; set; } = new List<XliffGroup>();
    }

    /// <summary>
    /// Represents a translation unit in XLIFF 2.0
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
        [XmlAttribute("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Approval state
        /// </summary>
        [XmlIgnore]
        public ApprovalState Approved { get; set; } = ApprovalState.Unapproved;

        /// <summary>
        /// XML serialization property for Approved
        /// </summary>
        [XmlAttribute("approved")]
        public string? ApprovedString
        {
            get => Approved == ApprovalState.Unapproved ? null : Approved.ToString().ToLowerInvariant();
            set => Approved = ParseApprovalState(value);
        }

        /// <summary>
        /// Notes about this translation unit
        /// </summary>
        [XmlElement("note")]
        public List<XliffNote> Notes { get; set; } = new List<XliffNote>();

        /// <summary>
        /// Collection of segments
        /// </summary>
        [XmlElement("segment")]
        public List<Segment> Segments { get; set; } = new List<Segment>();

        private static ApprovalState ParseApprovalState(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return ApprovalState.Unapproved;

            return value.ToLowerInvariant() switch
            {
                "approved" => ApprovalState.Approved,
                "unapproved" => ApprovalState.Unapproved,
                _ => ApprovalState.Unapproved
            };
        }
    }

    /// <summary>
    /// Represents a segment in XLIFF 2.0
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// Segment identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Translation state
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

        private static TranslationState ParseTranslationState(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return TranslationState.New;

            return value.ToLowerInvariant() switch
            {
                "initial" => TranslationState.New,
                "translated" => TranslationState.Translated,
                "reviewed" => TranslationState.NeedsReviewTranslation,
                "final" => TranslationState.Final,
                _ => TranslationState.New
            };
        }
    }

    /// <summary>
    /// Represents a note in XLIFF 2.0
    /// </summary>
    public class XliffNote
    {
        /// <summary>
        /// Note identifier
        /// </summary>
        [XmlAttribute("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Note priority
        /// </summary>
        [XmlAttribute("priority")]
        public string? Priority { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        [XmlAttribute("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Note content
        /// </summary>
        [XmlText]
        public string Content { get; set; } = string.Empty;
    }
}