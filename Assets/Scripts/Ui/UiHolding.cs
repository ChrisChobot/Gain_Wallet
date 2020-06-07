using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiHolding : MonoBehaviour
{
    public TMP_Text HoldingName;
    public TMP_Text CurrentIncome;
    public TMP_Text CurrentPrice;
    public TMP_Text Stock;
    public TMP_Text ActualizationDate;
    private UiManager _uiManager;
    private Holding _holding;

    public void Init(UiManager uiManager, Holding holding, int count)
    {
        _holding = holding;
        _uiManager = uiManager;
        HoldingName.text = holding.Name;
        string date = DateTime.ParseExact(holding.Date,
                                        "yyyyMMdd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None)
                                        .ToString("dd/MM/yyyy");
        CurrentIncome.text = $"Current income:{holding.OverallGain.ToString()}{UiManager.Currency}";
        CurrentPrice.text = $"Current price:{holding.Close.ToString()}{UiManager.Currency}";
        Stock.text = $"Stock:{count}";
        ActualizationDate.text = $"Actualization date:{date}";
    }
    public void DeleteHolding()
    {
        _uiManager.ConfirmationWindow.Show(() => { _uiManager.UiMainMenu.HoldingsManager.RemoveUserHolding(_holding);
                                                    _uiManager.UpdateTotalEarnings();
                                                    _uiManager.UiHoldings.CreatedHoldings = false;
                                                    _uiManager.UiHoldings.Show(true);
                                                }, () => { }, "Are you sure about deleting whole holding?");
       
    }

    public void Edit()
    {
        _uiManager.UiMainMenu.AddingNew.Open(_holding);
    }
}
