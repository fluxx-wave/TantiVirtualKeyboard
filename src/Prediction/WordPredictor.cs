using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotVirtualKeyboard.Prediction
{
    public class WordPredictor
    {
        private static readonly Lazy<WordPredictor> _instance = new Lazy<WordPredictor>(CreateInstance);
        private static WordPredictor CreateInstance()
        {
            return new WordPredictor();
        }

        public static WordPredictor Instance
        {
            get { return _instance.Value; }
        }

        private readonly Dictionary<string, int> _wordFrequencies;
        private readonly StringBuilder _currentWordBuffer;

        public event EventHandler<IReadOnlyList<string>> SuggestionsUpdated;

        public string CurrentPrefix
        {
            get { return _currentWordBuffer.ToString(); }
        }

        public WordPredictor()
        {
            _wordFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _currentWordBuffer = new StringBuilder();

            // Populate initial dictionary with frequencies
            int rank = 10000;
            foreach (var word in DictionaryData.CommonWords)
            {
                _wordFrequencies[word] = rank--;
            }
        }

        public void AppendChar(char ch)
        {
            if (char.IsLetter(ch) || ch == '\'' || ch == '-')
            {
                _currentWordBuffer.Append(ch);
                UpdateSuggestions();
            }
            else
            {
                // Word boundary reached (space, punctuation, enter)
                CommitCurrentWord();
            }
        }

        public void OnBackspace()
        {
            if (_currentWordBuffer.Length > 0)
            {
                _currentWordBuffer.Length--;
                UpdateSuggestions();
            }
            else
            {
                ClearSuggestions();
            }
        }

        public void CommitCurrentWord()
        {
            if (_currentWordBuffer.Length >= 2)
            {
                string word = _currentWordBuffer.ToString().ToLowerInvariant();
                LearnWord(word);
            }
            _currentWordBuffer.Clear();
            ClearSuggestions();
        }

        public void LearnWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 2) return;

            string lower = word.ToLowerInvariant();
            int count;
            if (_wordFrequencies.TryGetValue(lower, out count))
            {
                _wordFrequencies[lower] = count + 10;
            }
            else
            {
                _wordFrequencies[lower] = 100;
            }
        }

        public IReadOnlyList<string> GetSuggestions(int maxCount)
        {
            string prefix = _currentWordBuffer.ToString().ToLowerInvariant();
            if (string.IsNullOrEmpty(prefix))
            {
                return new List<string> { "the", "and", "is", "for", "you", "that" };
            }

            var matches = _wordFrequencies
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(maxCount)
                .ToList();

            // Match case of the typed prefix
            bool isUpper = _currentWordBuffer.Length > 0 && char.IsUpper(_currentWordBuffer[0]);
            if (isUpper)
            {
                matches = matches.Select(m => char.ToUpper(m[0]) + (m.Length > 1 ? m.Substring(1) : "")).ToList();
            }

            return matches;
        }

        public IReadOnlyList<string> GetSuggestions()
        {
            return GetSuggestions(6);
        }

        private void UpdateSuggestions()
        {
            var suggestions = GetSuggestions();
            EventHandler<IReadOnlyList<string>> handler = SuggestionsUpdated;
            if (handler != null)
            {
                handler(this, suggestions);
            }
        }

        private void ClearSuggestions()
        {
            EventHandler<IReadOnlyList<string>> handler = SuggestionsUpdated;
            if (handler != null)
            {
                handler(this, new List<string>());
            }
        }
    }
}
