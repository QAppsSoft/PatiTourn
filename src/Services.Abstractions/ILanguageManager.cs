using System.Collections.Generic;
using Services.Abstractions.Models;

namespace Services.Abstractions
{
    public interface ILanguageManager
    {
        LanguageModel CurrentLanguage { get; }

        LanguageModel DefaultLanguage { get; }

        IEnumerable<LanguageModel> AllLanguages { get; }

        void SetLanguage(string languageCode);

        void SetLanguage(LanguageModel languageModel);
    }
}
