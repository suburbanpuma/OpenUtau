using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenUtau.Api;
using Serilog;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// Spanish [e ny e] compatible dictionary.
    /// <para>
    /// [e ny e] uses a custom phonetic system that is backwards compatible with the old CVVC Spanish encoding.
    /// [e ny e] is completely diphonic, and triphones (ej. bla) are not supported. VCV Spanish is not supported.
    /// You can use your favorite classic CVVC/VCV Spanish banks with the Spanish Syllable-Based Phonemizer, by Lotte V. You should give her all ur money btw, she makes great stuff.
    /// </para>
    /// </summary>
    [Phonemizer("[e ny e] Phonemizer", "ES [NY]", "subpum")]
    public class EnyePhonemizer : LatinDiphonePhonemizer {
        public EnyePhonemizer() {
            try {
                Initialize();
            } catch (Exception e) {
                Log.Error(e, "Failed to initialize.");
            }
        }

        protected override IG2p LoadG2p() {
            var g2ps = new List<IG2p>();

            // Load phonemes from plugin folder.
            string path = Path.Combine(PluginDir, "phonemes.yaml");
            if (!File.Exists(path)) {
                Directory.CreateDirectory(PluginDir);
                File.WriteAllBytes(path, Data.Resources.arpasing_template);
            }
            g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(path)).Build());
            
            // Attempt to load dictionary from Dictionaries folder.
            protected override string GetDictionaryName() => "dict.json";
            protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => dictionaryReplacements;

            // Load dictionary from singer folder.
            if (singer != null && singer.Found && singer.Loaded) {
                string file = Path.Combine(singer.Location, "dict.json");
                if (File.Exists(file)) {
                    try {
                        g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(file)).Build());
                    } catch (Exception e) {
                        Log.Error(e, $"Failed to load {file}");
                    }
                }
            }

            // Load base g2p.
            g2ps.Add(new ArpabetG2p());

            return new G2pFallbacks(g2ps.ToArray());
        }

        protected override Dictionary<string, string[]> LoadVowelFallbacks() {
            return "aa=ah,ae;ae=ah,aa;ah=aa,ae;ao=ow;ow=ao;eh=ae;ih=iy;iy=ih;uh=uw;uw=uh;aw=ao".Split(';')
                .Select(entry => entry.Split('='))
                .ToDictionary(parts => parts[0], parts => parts[1].Split(','));
        }
    }
}
