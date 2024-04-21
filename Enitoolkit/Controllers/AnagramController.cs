using Enitoolkit.Dictionaries;
using Enitoolkit.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Enitoolkit.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class AnagramController : ControllerBase
    {
        private readonly ILogger<AnagramController> _logger;
        private readonly IDictionaryService _dictionary;

        public AnagramController(ILogger<AnagramController> logger, IDictionaryService dictionary)
        {
            _logger = logger;
            _dictionary = dictionary;
        }

        // GET: /anagram?key=
        [HttpGet]
        [ActionName("anagram")]
        [ProducesResponseType<Dictionary<int, List<string>>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IResult Get(string key)
        {
            if (!_dictionary.Loaded)
            {
                return Results.Problem("Anagram Service Unavailable.");
            }
            else
            {
                var tmp = SolveAnagram(key);
                if (tmp == null)
                    return Results.Problem("Given key could not be processed.");

                return Results.Ok(tmp);
            }
        }

        // GET: /anagram/exact?key=
        [HttpGet]
        [ActionName("anagram/exact")]
        [ProducesResponseType<Dictionary<int, List<string>>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IResult GetExact(string key)
        {
            if (!_dictionary.Loaded)
            {
                return Results.Problem("Anagram Service Unavailable.");
            }
            else
            {
                var tmp = SolveAnagram(key, true);
                if (tmp == null)
                    return Results.Problem("Given key could not be processed.");

                return Results.Ok(tmp);
            }
        }

        /// <summary>
        /// Method <c>SolveAnagram</c> solves all anagrams of a given key (including shorter words and wildcards support). <br></br>
        /// It can work in normal mode or exact mode. <br></br>
        /// Normal mode returns all words that can be made from given letters, also shorter ones. <br></br>
        /// Exact mode returns only words of the same length as key.
        /// </summary>
        /// <param name="key">String that is used to solve anagrams.</param>
        /// <param name="exact_mode">Whether solver should work in normal mode or exact mode.</param>
        /// <returns>
        /// Dictionary of solved anagrams grouped by their lengths,<br></br> 
        /// empty dictionary if none were found or<br></br> 
        /// <c>null</c> if the key couldn't be processed.
        /// </returns>
        private Dictionary<int, List<string>>? SolveAnagram(string key, bool exact_mode = false)
        {
            if (key.All(c => Char.IsLetter(c) || c == '*')) {
                // lowercase then count occurences of each letter in provided key
                var sorted_key = key.ToLower();
                var key_len = sorted_key.Length;
                var key_letter_counts = sorted_key.GroupBy(c => c)
                    .Select(c => new { Char = c.Key, Count = c.Count() })
                    .ToDictionary(x => x.Char, x => x.Count);

                var key_letters = new HashSet<char>(key_letter_counts.Keys);

                var found_anagrams = new Dictionary<int, List<string>>();

                IEnumerable<DictionaryItem> potential_anagrams;

                // request potential anagrams from dictionary
                if (exact_mode)
                    potential_anagrams = _dictionary.GetExact(key_len);
                else
                    potential_anagrams = _dictionary.Get(key_len);

                foreach (var item in potential_anagrams)
                {
                    
                    if (!CheckIfAnagramWithWildcard(key_letters, key_letter_counts, item))
                        continue;

                    // if all conditions are satisfied, add new anagrams based on letter count of item
                    if (!found_anagrams.ContainsKey(item.Key.Length))
                    {
                        found_anagrams[item.Key.Length] = item.Words;
                    }
                    else
                        found_anagrams[item.Key.Length].AddRange(item.Words);
                }

                return found_anagrams;

            } else
                return null;

        }
        /// <summary>
        /// Method <c>CheckIfAnagram</c> is similar to <see cref="CheckIfAnagramWithWildcard(HashSet{char}, Dictionary{char, int}, DictionaryItem)"/> but without the support for wildcards ('*').
        /// </summary>
        /// <param name="key_letters">See <see cref="CheckIfAnagramWithWildcard(HashSet{char}, Dictionary{char, int}, DictionaryItem)"/></param>
        /// <param name="key_letter_counts">See <see cref="CheckIfAnagramWithWildcard(HashSet{char}, Dictionary{char, int}, DictionaryItem)"/></param>
        /// <param name="item">See <see cref="CheckIfAnagramWithWildcard(HashSet{char}, Dictionary{char, int}, DictionaryItem)"/></param>
        /// <returns>See <see cref="CheckIfAnagramWithWildcard(HashSet{char}, Dictionary{char, int}, DictionaryItem)"/></returns>
        private bool CheckIfAnagram(HashSet<char> key_letters, Dictionary<char, int> key_letter_counts, DictionaryItem item)
        {
            var item_letters = new HashSet<char>(item.Letters.Keys);

            // check if the same letters occur in both
            if (!item_letters.IsSubsetOf(key_letters))
                return false;

            // check if all letter counts in item are lower or equal to those in key 
            if (item_letters.Any(l => item.Letters[l] > key_letter_counts[l]))
                return false;

            return true;
        }

        /// <summary>
        /// Method <c>CheckIfAnagramWithWildcard</c> checks whether dictionary item can be made with letters of the key, with the support for wildcards ('*').
        /// </summary>
        /// <param name="key_letters"><c>HashSet</c> of letters in key</param>
        /// <param name="key_letter_counts">Counted occurences of letters in key</param>
        /// <param name="item">Dictionary item that is to be checked against key</param>
        /// <returns>
        /// <c>true</c> if dictionary item can be made from provided letters,<br></br>
        /// <c>false</c> if not.
        /// </returns>
        private bool CheckIfAnagramWithWildcard(HashSet<char> key_letters, Dictionary<char, int> key_letter_counts, DictionaryItem item)
        {
            // determine if we're dealing with wildcards or not
            if (!key_letters.Contains('*'))
                return CheckIfAnagram(key_letters, key_letter_counts, item);

            // create hashset of letters from dict index
            var item_letters = new HashSet<char>(item.Letters.Keys);

            // get common letters between key and item
            var intersection = item_letters.Intersect(key_letters).ToHashSet<char>();

            if (intersection.Count == 0)
                return false;

            // count how many occurances of the same letters there are in common
            int wildcard_sum = 0;
            foreach ( var letter in intersection)
            {
                wildcard_sum += Math.Min(key_letter_counts[letter], item.Letters[letter]);
            }

            // check if the word can be made with the addition of provided wildcards
            if (wildcard_sum + key_letter_counts['*'] < item.Length)
                return false;

            return true;
        }
    }
}
