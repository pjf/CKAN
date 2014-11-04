using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    /// This class describes module install "stanzas" as found in CKAN documents.
    /// </summary>

    public class ModuleInstallDescriptor
    {
        public /* required */ string file;
        public /* required */ string install_to;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> filter_regexp;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> include_only;

        [JsonConverter(typeof (JsonSingleOrArrayConverter<string>))]
        public List<string> include_only_regexp;

        [OnDeserialized]
        internal void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Make sure our required fields exist.
            if (file == null || install_to == null)
            {
                throw new BadMetadataKraken(null, "Install stanzas must have a file and install_to");
            }

            // I don't know why we end up with uninitialised fields here if they're absent
            // from the metadata, but resources seems to. In any case, make sure they're
            // all set to something sensible.

            filter              = filter              ?? new List<string>();
            filter_regexp       = filter_regexp       ?? new List<string>();
            include_only        = include_only        ?? new List<string>();
            include_only_regexp = include_only_regexp ?? new List<string>();

            if (
                // Don't mix filters and includes, mmkay?
                (filter.Count != 0       || filter_regexp.Count != 0) &&
                (include_only.Count != 0 || include_only_regexp.Count != 0)
            )
            {
                throw new BadMetadataKraken(null, "Mixed filters and include stanzas in install section");
            }
        }

        /// <summary>
        /// Returns true if the path provided should be installed by this stanza.
        /// </summary>
        public bool IsWanted(string path)
        {
            // Make sure our path always uses slashes we expect.
            string normalised_path = path.Replace('\\', '/');

            // We want everthing that matches our 'file', either as an exact match,
            // or as a path leading up to it.
            string wanted_filter = "^" + Regex.Escape(this.file) + "(/|$)";

            // If it doesn't match our install path, ignore it.
            if (! Regex.IsMatch(normalised_path, wanted_filter))
            {
                return false;
            }

            // Skip the file if it's a ckan file, these should never be copied to GameData.
            if (Regex.IsMatch(normalised_path, ".ckan$", RegexOptions.IgnoreCase))
            {
                return false;
            }

            // Get all our path segments. If our filter matches of any them, skip.
            // All these comparisons are case insensitive.
            var path_segments = new List<string>(normalised_path.ToLower().Split('/'));

            foreach (string filter_text in this.filter)
            {
                if (path_segments.Contains(filter_text.ToLower()))
                {
                    return false;
                }
            }

            // Finally, check our filter regexpes.
            foreach (string regexp in this.filter_regexp)
            {
                if (Regex.IsMatch(normalised_path, regexp))
                {
                    return false;
                }
            }

            // I guess we want this file after all. ;)
            return true;
        }
    }
}