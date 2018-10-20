﻿namespace GitHub.Models
{
    /// <summary>
    /// Model for a single check annotation.
    /// </summary>
    public class CheckRunAnnotationModel
    {
        /// <summary>
        /// The path to the file that this annotation was made on.
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// The starting line number (1 indexed).
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// The ending line number (1 indexed).
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// The path that this annotation was made on.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The annotation's message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The annotation's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The annotation's severity level.
        /// </summary>
        public CheckAnnotationLevel? AnnotationLevel { get; set; }

        /// <summary>
        /// Additional information about the annotation.
        /// </summary>
        public string RawDetails { get; set; }
    }
}