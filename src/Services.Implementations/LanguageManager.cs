using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Extensions;
using Services.Abstractions;
using Services.Abstractions.Models;

namespace Services.Implementations
{
    public class LanguageManager : ILanguageManager
    {
        private readonly Lazy<Dictionary<string, LanguageModel>> _availableLanguages;

        public LanguageManager()
        {
            _availableLanguages = new Lazy<Dictionary<string, LanguageModel>>(GetAvailableLanguages);
            DefaultLanguage = CreateLanguageModel(CultureInfo.GetCultureInfo("en"));
        }

        public LanguageModel DefaultLanguage { get; }

        public LanguageModel CurrentLanguage => CreateLanguageModel(Thread.CurrentThread.CurrentUICulture);

        public IEnumerable<LanguageModel> AllLanguages => _availableLanguages.Value.Values;

        public void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                throw new ArgumentException($"{nameof(languageCode)} can't be empty.");
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(languageCode);
        }

        public void SetLanguage(LanguageModel languageModel)
        {
            SetLanguage(languageModel.Code);
        }

        private Dictionary<string, LanguageModel> GetAvailableLanguages()
        {
            return SupportedLanguages()
                .Select(locale => CreateLanguageModel(new CultureInfo(locale)))
                .ToDictionary(lm => lm.Code, lm => lm);
        }

        private static IEnumerable<string> SupportedLanguages()
        {
            return new[] { "en", "es" };
        }

        private LanguageModel CreateLanguageModel(CultureInfo cultureInfo)
        {
            return cultureInfo is null
                ? DefaultLanguage
                : new LanguageModel(cultureInfo.EnglishName, cultureInfo.NativeName.ToTitleCase(),
                    cultureInfo.TwoLetterISOLanguageName);
        }
    }
}
