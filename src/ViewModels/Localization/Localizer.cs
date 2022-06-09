using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using Languages;

namespace ViewModels.Localization
{
    public class Localizer : INotifyPropertyChanged
    {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private Dictionary<string, string> _translations = null;

        public Localizer()
        {

        }

        public bool LoadLanguage(string language)
        {
            Language = language;

            var myManager = new ResourceManager(typeof(Default));
            var resourceSet = myManager.GetResourceSet(new CultureInfo(language), false, true);

            if (resourceSet == null)
            {
                return false;
            }

            _translations = resourceSet.Cast<DictionaryEntry>()
                .ToDictionary(
                    entry => entry.Key.ToString(),
                    entry => entry.Value != null ? entry.Value.ToString() : entry.Key.ToString(),
                    StringComparer.Ordinal);

            Invalidate();

            return true;
        } // LoadLanguage

        public string Language { get; private set; }

        public string this[string key]
        {
            get
            {
                string res;
                if (_translations != null && _translations.TryGetValue(key, out res))
                    return res.Replace("\\n", "\n");

                return $"{Language}:{key}";
            }
        }

        public static Localizer Instance { get; set; } = new Localizer();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
