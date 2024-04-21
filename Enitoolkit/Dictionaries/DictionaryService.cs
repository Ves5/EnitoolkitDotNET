using Enitoolkit.Models;
using System.Text.Json;

namespace Enitoolkit.Dictionaries
{
    /// <summary>
    /// An interface to request dictionary data used for anagram service.
    /// </summary>
    public interface IDictionaryService
    {
        /// <summary>
        /// Gets the state of dictionary.
        /// </summary>
        public bool Loaded { get; }

        /// <summary>
        /// Method <c>GetAll</c> requests for whole dictionary data.
        /// </summary>
        /// <returns>
        /// An <c>IEnumerable</c> with whole stored dictionary data.<br></br>
        /// Each item in <c>DictionaryItem</c> format.
        /// </returns>
        public IEnumerable<DictionaryItem> GetAll();

        /// <summary>
        /// Method <c>Get</c> reqeusts a subset of the dictionary, with only items of specified length or lower.
        /// </summary>
        /// <param name="maxLen">Maximal key length of requested dictionary items.</param>
        /// <returns>
        /// An <c>IEnumerable</c> with a specified subset of the dictionary.<br></br>
        /// Each item in <c>DictionaryItem</c> format.
        /// </returns>
        public IEnumerable<DictionaryItem> Get(int maxLen);

        /// <summary>
        /// Method <c>GetExact</c> requests a subset of dictionary, with only items of specified length.
        /// </summary>
        /// <param name="exactLen">Exact key length of requested dictionary items</param>
        /// <returns>
        /// An <c>IEnumerable</c> with a specified subset of the dictionary.<br></br>
        /// Each item in <c>DictionaryItem</c> format.
        /// </returns>
        public IEnumerable<DictionaryItem> GetExact(int exactLen);

        /// <summary>
        /// Method <c>LoadDictionary</c> imports dictionary index from a file, 
        /// converts its items to <c>DictionaryItem</c> and stores the data in memory.
        /// </summary>
        public void LoadDictionary();
    }


    public class DictionaryService : IDictionaryService
    {
        private static List<DictionaryItem> _dictionaryItems = new();
        private static int _maxKeyLength = 0;
        private static bool loaded = false;
        private readonly ILogger<DictionaryService> _logger;

        public bool Loaded { get => loaded; private set => loaded = value; }
        public DictionaryService(ILogger<DictionaryService> logger) {
            _logger = logger;
        }
        public IEnumerable<DictionaryItem> GetAll()
        {
            _logger.LogDebug(0, "Returning all of the dictionary items.");
            return _dictionaryItems;
        }

        public IEnumerable<DictionaryItem> Get(int maxLen)
        {
            if (maxLen >= _maxKeyLength)
                return GetAll();

            _logger.LogDebug(0, $"Returning dictionary items with max key length {maxLen}.");
            return _dictionaryItems.Where(x => x.Length <= maxLen);
        }

        public IEnumerable<DictionaryItem> GetExact(int exactLen)
        {
            _logger.LogDebug(0, $"Returning dictionary items with exact key length {exactLen}.");
            return _dictionaryItems.Where(x => x.Length == exactLen);
        }

        public void LoadDictionary()
        {
            _logger.LogInformation(1, "Loading dictionary index.");
            try
            {
                var path = Environment.CurrentDirectory + "/Dictionaries/wwf_index.json";
                string text = System.IO.File
                    .ReadAllText(path);
                _logger.LogDebug(1, $"Dictionary Index loaded from path: {path}");
                var dict_index = JsonSerializer
                    .Deserialize<Dictionary<string, DictionaryImportItem>>(text);

                _logger.LogDebug(1, "Commencing dictionary item conversion.");
                foreach(var item in dict_index)
                {
                    _dictionaryItems.Add(new DictionaryItem()
                    {
                        Key = item.Key,
                        Length = item.Key.Length,
                        Letters = item.Value.Letters,
                        Words = item.Value.Words
                    });
                    _maxKeyLength = Math.Max(_maxKeyLength, item.Key.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, ex.Message);
            }
            _logger.LogDebug(1, $"Correctly loaded {_dictionaryItems.Count} items; longest key: {_maxKeyLength}.");
            loaded = true;
        }
    }
}
