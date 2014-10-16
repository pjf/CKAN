namespace CKAN {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using log4net;

    /// <summary>
    /// Utility class to track version -> module mappings
    /// </summary>
    public class AvailableModule {

        // The map of versions -> modules, that's what we're about!
        // This *has* to be public, as it's generated by a JSON deserialiser
        public Dictionary<Version,CkanModule> module_version = new Dictionary<Version, CkanModule> ();

        private static readonly ILog log = LogManager.GetLogger(typeof(AvailableModule));

        public AvailableModule() { }

        public void Add(CkanModule module) {
            log.DebugFormat ("Adding {0}", module);
            module_version[module.version] = module;
        }

        /// <summary>
        /// Return the most recent release of a module.
        /// Optionally takes a KSP version number to target.
        /// Returns null if there are no compatible versions.
        /// </summary>
        public CkanModule Latest(KSPVersion ksp_version = null) {
            var available_versions = new List<Version> (module_version.Keys);

            log.DebugFormat ("Our dictionary has {0} keys", module_version.Keys.Count);
            log.DebugFormat ("Choosing between {0} available versions", available_versions.Count);

            // Sort most recent versions first.

            available_versions.Sort ();
            available_versions.Reverse();

            if (ksp_version == null) {
                CkanModule module = module_version [available_versions.First ()];

                log.DebugFormat ("No KSP version restriction, {0} is most recent", module);
                return module;
            }

            // Time to check if there's anything that we can satisfy.

            foreach (Version v in available_versions) {
                if (module_version[v].IsCompatibleKSP (ksp_version)) {
                    return module_version[v];
                }
            }

            log.DebugFormat ("No version of {0} is compatible with KSP {1}", module_version [available_versions[0]].identifier, ksp_version);

            // Oh noes! Nothing available!
            return null;

        }
    }
}

