using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Serialization.Wire;

namespace Base.SaveSystemPackage.Core
{
    /// <summary>
    /// One decoded save: its metadata and its state blob, or the reason reading them failed. Lets a
    /// load try the live files and then each backup in turn through the same shape.
    /// </summary>
    internal readonly struct DecodedSave
    {
        /// <summary>The decoded metadata, or <c>null</c> when the save could not be read.</summary>
        internal SaveMetadataDto Metadata { get; }

        /// <summary>The decoded state blob, or <c>null</c> when the save could not be read.</summary>
        internal SaveBlob Blob { get; }

        /// <summary>Why reading failed. Meaningless once <see cref="IsComplete"/> is true.</summary>
        internal ESaveLoadResult Result { get; }

        /// <summary>True when both parts were read and the save can be applied.</summary>
        internal bool IsComplete => Metadata != null && Blob != null;

        /// <summary>Creates a successfully decoded save.</summary>
        /// <param name="metadata">The decoded metadata.</param>
        /// <param name="blob">The decoded state blob.</param>
        internal DecodedSave(SaveMetadataDto metadata, SaveBlob blob)
        {
            Metadata = metadata;
            Blob = blob;
            Result = ESaveLoadResult.Success;
        }

        /// <summary>Creates a failed read carrying the reason.</summary>
        /// <param name="result">Why the save could not be read.</param>
        /// <returns>An incomplete decoded save.</returns>
        internal static DecodedSave Failed(ESaveLoadResult result) => new(result);

        private DecodedSave(ESaveLoadResult result)
        {
            Metadata = null;
            Blob = null;
            Result = result;
        }
    }
}