using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiNewHolding : MonoBehaviour
{
    public HoldingsManager HoldingsManager;
    public UiManager UiManager;
    public TMP_Dropdown Names;
    public TMP_Text NamesName;
    public InputField WhenBuyed;
    public TMP_Text WhenBuyedName;
    public InputField HowMany;
    public TMP_Text HowManyName;
    public GameObject PriceButton;
    public InputField Price;
    public TMP_Text PriceName;
    public TMP_Text WindowName;
    public TMP_Text MoneySpent;
    public TMP_Text AddButtonText;
    public GameObject BeforeWindow;
    private bool _prepareForEdit;

    public Image Blockade;
    private UiNewHoldingWindowType _currentWindowType;

    private void Start()
    {
        ClearInputs();
        Names.value = 0;
        Names.options = GetAllTNSOptions();
    }

    private void Update() // do przerobienia na event
    {
        if (!string.IsNullOrEmpty(HowMany.text) && !string.IsNullOrEmpty(Price.text))
        {
            MoneySpent.text = $"Money spent: {int.Parse(HowMany.text) * float.Parse(Price.text)}{UiManager.Currency}";
        }
    }

    private List<TMP_Dropdown.OptionData> GetAllTNSOptions()
    {
        List<TMP_Dropdown.OptionData> result = new List<TMP_Dropdown.OptionData>();
        List<TickerNameSystem> TickersAndNames = HoldingsManager.GetTickersAndNames();

        foreach (var item in TickersAndNames)
        {
            result.Add(new TMP_Dropdown.OptionData(item.Name));
        }

        return result;
    }

    private List<TMP_Dropdown.OptionData> GetFilteredTNSOptions()
    {
        List<TMP_Dropdown.OptionData> result = new List<TMP_Dropdown.OptionData>();
        List<TickerNameSystem> TickersAndNames = HoldingsManager.GetUserTickerAndNames();

        foreach (var item in TickersAndNames)
        {
            result.Add(new TMP_Dropdown.OptionData(item.Name));
        }

        return result;
    }

    public void Open(Holding holding)
    {
        BeforeWindow.SetActive(true);
        gameObject.SetActive(true);
        Block(false);
        int index = Names.options.FindIndex((x) => x.text == holding.Name);
        if (index >= 0)
        {
            Names.value = index;
        }
        HowMany.text = DateTime.ParseExact(holding.Date,
                                        "yyyyMMdd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None)
                                        .ToString("dd/MM/yyyy");

        _prepareForEdit = true;
    }

    public void Open()
    {
        BeforeWindow.SetActive(true);
        gameObject.SetActive(true);
        Block(false);
        _prepareForEdit = false;
    }

    public void UserWantToAdd()
    {
        Open(UiNewHoldingWindowType.AddNew);
        BeforeWindow.SetActive(false);
    }

    public void UserWantToDelete()
    {
        Open(UiNewHoldingWindowType.Delete);
        BeforeWindow.SetActive(false);
    }

    public void Open(UiNewHoldingWindowType windowsType)
    {
        _currentWindowType = windowsType;

        switch (windowsType)
        {
            case UiNewHoldingWindowType.AddNew:
                Names.options = GetAllTNSOptions();
                WindowName.text = "Adding holding";
                AddButtonText.text = "Add";
                Names.interactable = !_prepareForEdit;
                Names.gameObject.SetActive(true);
                NamesName.gameObject.SetActive(true);
                WhenBuyed.gameObject.SetActive(true);
                WhenBuyedName.gameObject.SetActive(true);
                Price.gameObject.SetActive(true);
                PriceName.gameObject.SetActive(true);
                PriceButton.SetActive(true);
                break;
            case UiNewHoldingWindowType.Delete:
                if (!_prepareForEdit)
                {
                    Names.options = GetFilteredTNSOptions();
                }
               
                WindowName.text = "Delete holdings";
                AddButtonText.text = "Delete";
                Names.interactable= !_prepareForEdit;
                Names.gameObject.SetActive(true);
                NamesName.gameObject.SetActive(true);
                WhenBuyed.gameObject.SetActive(false);
                WhenBuyedName.gameObject.SetActive(false);
                Price.gameObject.SetActive(false);
                PriceName.gameObject.SetActive(false);
                PriceButton.SetActive(false);
                break;
        }

        ClearInputs();
        WhenBuyed.text = DateTime.Today.ToString("d");
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Block(false);
    }

    public void DownloadPrice()
    {
        DateTime date;
        if (Names.value >= 0 && SanitizeWhenBuyed(out date) && date <= DateTime.Today)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday) 
            {
                date.AddDays(-1);
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date.AddDays(-2);
            }

            Block(true);
            StartCoroutine(DownloadPriceCoroutine(date));
        }
        else
        {
            //todo "select holding and type day!" error
        }
    }

    private IEnumerator DownloadPriceCoroutine(DateTime date)
    {
        yield return new WaitForEndOfFrame();
        
        Holding holding = HoldingsManager.GetHoldingAt(Names.options[Names.value].text, date);
        if (holding != null)
        {
            Price.text = ((holding.High + holding.Low) / 2).ToString();
            yield return new WaitForEndOfFrame();
        }
        else
        {
            // TODO: In this date, there wasn't company holdings
            Price.text = "0,00";
        }

        Block(false);
        yield return new WaitForEndOfFrame();
    }

    private void ClearInputs()
    {
        WhenBuyed.text = string.Empty;
        HowMany.text = string.Empty;
        Price.text = string.Empty;
        MoneySpent.text = string.Empty;
    }

    private bool SanitizeWhenBuyed(out DateTime date)
    {
        string whenBuyed = RemoveExtraText(WhenBuyed.text);
        WhenBuyed.text = whenBuyed;

        if (DateTime.TryParse(whenBuyed,out date))
        {
            return true;
        }
        else
        {
            return false;
        }

        string RemoveExtraText(string value)
        {
            var allowedChars = "0123456789/";
            return new string(value.Replace('\\','/').Replace('.','/').Replace('-','/').Where(c => allowedChars.Contains(c)).ToArray());
        }
    }

    private void Block(bool value)
    {
        if (Blockade.gameObject.activeSelf != value)
        {
            Blockade.gameObject.SetActive(value);
        }
    }

    public void TryAdd()
    {
        string holdingName = Names.options[Names.value].text;
        int howMany;

        if (_currentWindowType == UiNewHoldingWindowType.Delete)
        {
            Holding holding = HoldingsManager.GetUserHoldings().Keys.FirstOrDefault(uh => uh.Ticker == holdingName || uh.Name == holdingName);
            if (holding != null && !string.IsNullOrEmpty(HowMany.text) && int.TryParse(HowMany.text, out howMany) && howMany != 0 && HoldingsManager.GetUserHoldings()[holding] >= howMany) 
            {
                HoldingsManager.DeleteHoldingsAndUpdateOverallGain(holding, howMany);
            }
            else
            {
                //todo delete error
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(HowMany.text) && int.TryParse(HowMany.text, out howMany) && howMany != 0 && !string.IsNullOrEmpty(Price.text))
            {
                Block(true);
                if (SanitizeWhenBuyed(out DateTime date))
                {
                    Holding holding = HoldingsManager.GetHoldingAt(Names.options[Names.value].text, date);
                    HoldingsManager.UpdateOverallGain(holding, holdingName, howMany, float.Parse(Price.text));
                }

                Block(false);
            }
            else
            {
                //todo adding error
            }
        }

        UiManager.UiHoldings.CreatedHoldings = false;

        if (_prepareForEdit)
        {
            UiManager.UiHoldings.Show(true);
        }

        UiManager.UpdateTotalEarnings();
    }
}
