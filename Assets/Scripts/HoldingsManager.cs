using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using UnityEngine;

public class HoldingsManager : MonoBehaviour
{
    /// <summary>
    /// Contains Holdings belongs to User and amount of them 
    /// </summary>
    [SerializeField]
    private SerializableEventableDictionary<Holding, int> _userHoldings = new SerializableEventableDictionary<Holding, int>();

    /// <summary>
    /// All cached Holdings
    /// </summary>
    [SerializeField]
    private Dictionary<DateTime, List<Holding>> _downloadedHoldings = new Dictionary<DateTime, List<Holding>>();

    /// <summary>
    /// List with map Tickers to Names
    /// </summary>
    [SerializeField]
    private List<TickerNameSystem> _tickersAndNamesList = new List<TickerNameSystem>();

    public void Start()
    {
        _tickersAndNamesList = DownloadTickersAndNames();

        string fileWithSerializedData = Application.persistentDataPath + "/UH.xml";
        _userHoldings.FileName = fileWithSerializedData;
        _userHoldings.Deserialize();
        _userHoldings.OnDictionaryChanged += _serializationOnDictionaryChanged;
    }

    /// <summary>
    /// Returns copy of User Holdings Dictionary
    /// </summary>
    /// <returns></returns>
    public Dictionary<Holding, int> GetUserHoldings()
    {
        return new Dictionary<Holding, int>(_userHoldings);
    }

    /// <summary>
    /// Add new holding for user
    /// </summary>
    /// <param name="holding"></param>
    /// <param name="amount"></param>
    public void AddNewUserHolding(Holding holding, int amount, float? customPrice = null)
    {
        Holding existingHolding = _userHoldings?.FirstOrDefault(uh => uh.Key.Name == holding.Name || uh.Key.Ticker == holding.Ticker).Key;
        if (existingHolding == null)
        {
            _userHoldings.Add(holding, amount);
        }
        else
        {
            UpdateUserHoldingAmount(existingHolding, amount);
        }
    }


    /// <summary>
    /// Update user's holdings
    /// </summary>
    /// <param name="newHolding"></param>
    /// <param name="oldHolding"></param>
    /// <param name="amount">If < 0, ammount will not update</param>
    public void UpdateUserHolding(Holding oldHolding, Holding newHolding, int amount = -1, bool updateOverall = true)
    {
        if (_userHoldings.ContainsKey(oldHolding))
        {
            if (amount < 0)
            {
                amount = GetUserHoldingAmount(oldHolding);
            }

            RemoveUserHolding(oldHolding);
        }

        AddNewUserHolding(newHolding, amount);

        if (updateOverall)
        {
            UpdateOverallGain();
        }
    }

    /// <summary>
    /// Remove holding from user collection
    /// </summary>
    /// <param name="holding"></param>
    public void RemoveUserHolding(Holding holding)
    {
        _userHoldings.Remove(holding);
    }

    /// <summary>
    /// Gets amount of selected holding
    /// </summary>
    /// <param name="holding"></param>
    /// <returns></returns>
    public int GetUserHoldingAmount(Holding holding)
    {
        if (_userHoldings.ContainsKey(holding))
        {
            return _userHoldings[holding];
        }
        else
        {
            return -2;
        }
    }

    /// <summary>
    /// Update amount of selected holding
    /// </summary>
    /// <param name="holding"></param>
    /// <param name="amount"></param>
    public void UpdateUserHoldingAmount(Holding holding, int amount)
    {
        if (amount < -1)
        {
            amount = 0;
        }

        _userHoldings[holding] = amount;

        UpdateOverallGain();
    }

    /// <summary>
    /// Download ticker and names map list
    /// </summary>
    /// <returns>List with TickerNameSystem objects</returns>
    public List<TickerNameSystem> DownloadTickersAndNames()
    {
        string restOfData = FilesManager.DownloadRemoteFile("https://stooq.pl/db/l/?g=6", true);
        return FilesManager.ParseCsvToObject<TickerNameSystem>(restOfData, ' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Get list of all available Tickers and Names
    /// </summary>
    /// <returns></returns>
    public List<TickerNameSystem> GetTickersAndNames()
    {
        return new List<TickerNameSystem>(_tickersAndNamesList);
    }

    /// <summary>
    /// Get only Tickers and Names, belongs to user
    /// </summary>
    /// <returns></returns>
    public List<TickerNameSystem> GetUserTickerAndNames()
    {
        return _tickersAndNamesList.FindAll(tns => _userHoldings.Keys.FirstOrDefault(uhk => uhk.Name == tns.Name || uhk.Ticker == tns.Ticker) != null);
    }

    /// <summary>
    /// Get Holding at date for specific holding
    /// </summary>
    /// <param name="holdingName">Name or Ticker from Holding object</param>
    /// <param name="dateTime">Day to download data</param>
    /// <returns></returns>
    public Holding GetHoldingAt(string holdingName, DateTime dateTime)
    {
        DateTime parsedDateTime = GetDateBasedOnHoldingDefaultFormat(dateTime);

        if (parsedDateTime != DateTime.MinValue)
        {
            DownloadHoldingsIfMissing(ref parsedDateTime);

            List<Holding> holdingsAtCurrentDate = _downloadedHoldings[parsedDateTime];
            return holdingsAtCurrentDate?.FirstOrDefault(h => h.Name.Equals(holdingName) || h.Ticker.Equals(holdingName));
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Force downloading latest holding to cache
    /// </summary>
    public void ForceDownloadLatestHolding()
    {
        DateTime parsedDateTime = GetDateBasedOnHoldingDefaultFormat(DateTime.Now);

        if (parsedDateTime != DateTime.MinValue)
        {
            DownloadHoldingsIfMissing(parsedDateTime, true);
        }
    }

    /// <summary>
    /// Gets parsed Date to format with time 00:00:00
    /// </summary>
    /// <param name="dateTime">DateTime to reformat</param>
    /// <returns>DateTime.MinValue if parse fail</returns>
    public DateTime GetDateBasedOnHoldingDefaultFormat(DateTime dateTime)
    {
        string dateFormat = "yyyyMMdd";
        string onlyDate = $"{dateTime.Year}{dateTime.Month:00}{dateTime.Day:00}";
        bool parseResult = DateTime.TryParseExact(onlyDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime);

        return parseResult ? parsedDateTime : DateTime.MinValue;
    }

    /// <summary>
    /// Downloading Holding if not exists in cache. If data will not found, datetime will decrease by one day.
    /// </summary>
    /// <param name="dateTime">Latest datatime to start looking new data to download</param>
    /// <param name="forceDownload">Force download, even if it's in cache</param>
    public void DownloadHoldingsIfMissing(DateTime dateTime, bool forceDownload = false)
    {
        DownloadHoldingsIfMissing(ref dateTime, forceDownload);
    }
    /// <summary>
    /// Downloading Holding if not exists in cache. If data will not found, datetime will decrease by one day (with ref, to return achieved DateTime object).
    /// </summary>
    public void DownloadHoldingsIfMissing(ref DateTime dateTime, bool forceDownload = false)
    {
        DateTime lowestDataTimeValue = dateTime.AddMonths(-6);
        if (!_downloadedHoldings.ContainsKey(dateTime) || forceDownload)
        {
            while (!DownloadAllHoldingsAt(dateTime) || dateTime < lowestDataTimeValue)
            {
                dateTime = dateTime.AddDays(-1);
            }

            UpdateOverallGain();
        }
    }

    /// <summary>
    /// Updates all user holdings overall
    /// </summary>
    public void UpdateOverallGain()
    {
        DateTime lastData = _downloadedHoldings.Keys.Max();

        if (lastData != DateTime.MinValue)
        {
            List<Holding> holdings = _downloadedHoldings[lastData];

            foreach (var newHolding in holdings)
            {
                UpdateOverallGain(newHolding, false);
            }

            ForceSerialize(new Holding(), true);
        }
    }

    /// <summary>
    /// Update overall by price, holding amount and holding name/ticker
    /// </summary>
    /// <param name="holdingName">Handling name/ticker</param>
    /// <param name="count">Handling amount</param>
    /// <param name="price">Handling price</param>
    public void UpdateOverallGain(Holding holding, string holdingName, int count, float price, bool serializeAfterFinish = true)
    {
        Holding existingHolding = _userHoldings.Keys.FirstOrDefault(uh => uh.Ticker == holdingName || uh.Name == holdingName);

        if (existingHolding != null)
        {
            existingHolding.OverallGain += count * (existingHolding.Close - price);
            _userHoldings[existingHolding] += count;

            ForceSerialize(existingHolding, serializeAfterFinish);
        }
        else
        {
            holding.OverallGain = count * (holding.Close - price);
            _userHoldings.Add(holding, count);            
        }
    }

    /// <summary>
    /// Delete Holdings and update overall by amount and holding name/ticker
    /// </summary>
    /// <param name="holdingName">Handling name/ticker</param>
    /// <param name="count">Handling amount</param>
    /// <param name="price">Handling price</param>
    public void DeleteHoldingsAndUpdateOverallGain(Holding holding, int count, bool serializeAfterFinish = true)
    {
        if (holding != null)
        {
            holding.OverallGain -= holding.OverallGain * (count * 1f / _userHoldings[holding] * 1f);
            _userHoldings[holding] -= count;

            ForceSerialize(holding, serializeAfterFinish);
        }
    }

    /// <summary>
    /// Update userHoldings by new holding object
    /// </summary>
    /// <param name="newHolding">Holding object to update by</param>
    public void UpdateOverallGain(Holding newHolding, bool serializeAfterFinish = true)
    {
        Holding holding = _userHoldings.Keys.FirstOrDefault(uh => uh.Ticker == newHolding.Ticker || uh.Name == newHolding.Name);

        if (holding != null)
        {
            if (holding.LastOverallGainUpdate < newHolding.Timestamp)
            {
                holding.LastOverallGainUpdate = newHolding.Timestamp;
                holding.OverallGain += GetUserHoldingAmount(holding) * (newHolding.Close - holding.Close);
                holding.Close = newHolding.Close;

                ForceSerialize(holding, serializeAfterFinish);
            }
        }
    }

    /// <summary>
    /// Download all holdings (with name) by defined date.
    /// </summary>
    /// <param name="dateTime">Holdings state by date</param>
    /// <param name="updateTNS">If true, TickeNameSystem will be update</param>
    /// <returns>Is downloaded data empty (after "name" filtering)</returns>
    public bool DownloadAllHoldingsAt(DateTime dateTime, bool updateTNS = false)
    {
        try
        {
            string dataDate = $"{dateTime.Year}{dateTime.Month:00}{dateTime.Day:00}";

            string host = $"https://stooq.pl/db/d/?d={dataDate}&t=d";
            host = $"http://qyalonb.cluster028.hosting.ovh.net/hckton/{dataDate}_d";    // Temporary URL, because database on stooq has been fail today :(
            Debug.Log(host);
            string newData = FilesManager.DownloadRemoteFile(host);

            List<Holding> holdings = FilesManager.ParseCsvToObject<Holding>(newData);

            if (_tickersAndNamesList?.Count == 0 || updateTNS)
            {
                _tickersAndNamesList = DownloadTickersAndNames();
            }

            FilesManager filesManager = new FilesManager("<TICKER>");
            filesManager.MergeData(holdings, _tickersAndNamesList);

            holdings.RemoveAll(h => h.Name == null);

            if (holdings.Count > 0)
            {
                if (_downloadedHoldings.ContainsKey(dateTime))
                {
                    _downloadedHoldings[dateTime] = holdings;
                }
                else
                {
                    _downloadedHoldings.Add(dateTime, holdings);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch(WebException ex)
        {
            Debug.LogWarning($"DownloadAllHoldingsAt Exception:\n{ex}");
            return false;
        }
    }

    /// <summary>
    /// Sum overall gain from all holdings
    /// </summary>
    public float GetAllHoldingsGain()
    {
        float sum = 0;

        foreach (var item in _userHoldings)
        {
            sum += item.Key.OverallGain;
        }

        return sum;
    }

    /// <summary>
    /// Serialize User Holdings Dictionary on each change.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _serializationOnDictionaryChanged(object sender, DictionaryChangedEventArgs<Holding, int> e)
    {
        _userHoldings.Serialize();
    }

    /// <summary>
    /// Force execute serialization
    /// </summary>
    /// <param name="holding">Holding, which cause serialize execution</param>
    /// <param name="serializeAfterFinish">Serialize will be only when this value is true</param>
    private void ForceSerialize(Holding holding, bool serializeAfterFinish)
    {
        if (serializeAfterFinish)
        {
            _serializationOnDictionaryChanged(this, new DictionaryChangedEventArgs<Holding, int>(holding, GetUserHoldingAmount(holding), Assets.Scripts.Enums.DictionaryActionType.UpdateValue));
        }
    }
}
