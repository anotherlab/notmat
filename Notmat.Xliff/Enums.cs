using System.Xml.Serialization;

namespace Notmat
{
    /// <summary>
    /// Represents the state of a translation unit in XLIFF files
    /// </summary>
    public enum TranslationState
    {
        /// <summary>
        /// Default state - no translation provided
        /// </summary>
        [XmlEnum("new")]
        New,

        /// <summary>
        /// Translation is in progress
        /// </summary>
        [XmlEnum("needs-translation")]
        NeedsTranslation,

        /// <summary>
        /// Translation is completed but needs review
        /// </summary>
        [XmlEnum("needs-review-translation")]
        NeedsReviewTranslation,

        /// <summary>
        /// Translation has been reviewed and approved
        /// </summary>
        [XmlEnum("translated")]
        Translated,

        /// <summary>
        /// Translation is final and approved
        /// </summary>
        [XmlEnum("final")]
        Final,

        /// <summary>
        /// Translation needs adaptation
        /// </summary>
        [XmlEnum("needs-adaptation")]
        NeedsAdaptation,

        /// <summary>
        /// Translation needs L10n review
        /// </summary>
        [XmlEnum("needs-l10n")]
        NeedsL10n,

        /// <summary>
        /// Translation needs review for adaptation
        /// </summary>
        [XmlEnum("needs-review-adaptation")]
        NeedsReviewAdaptation,

        /// <summary>
        /// Translation needs review for L10n
        /// </summary>
        [XmlEnum("needs-review-l10n")]
        NeedsReviewL10n,

        /// <summary>
        /// Translation is signed off
        /// </summary>
        [XmlEnum("signed-off")]
        SignedOff
    }

    /// <summary>
    /// Represents the XLIFF version
    /// </summary>
    public enum XliffVersion
    {
        /// <summary>
        /// XLIFF version 1.2
        /// </summary>
        Version12,

        /// <summary>
        /// XLIFF version 2.0
        /// </summary>
        Version20
    }

    /// <summary>
    /// Represents approval state for XLIFF 2.0
    /// </summary>
    public enum ApprovalState
    {
        /// <summary>
        /// Not approved
        /// </summary>
        [XmlEnum("unapproved")]
        Unapproved,

        /// <summary>
        /// Approved
        /// </summary>
        [XmlEnum("approved")]
        Approved
    }
}