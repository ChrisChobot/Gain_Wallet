using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public UiConfirmation ConfirmationWindow;
    public UiMainMenu UiMainMenu;
    public UiHoldings UiHoldings;
    public TMP_Text TotalEarnings;
    public static string Currency = "PLN";
    public static bool IsAddingWindowOpen;
    public static bool IsShowWindowOpen;
    public Image Blockade;

    public void Start()
    {
        UpdateTotalEarnings();
    }

    public void UpdateTotalEarnings()
    {
        float value = UiMainMenu.HoldingsManager.GetAllHoldingsGain();
        string textValue;

        if (value > 0)
        {
            textValue = $"<color=#43b933>{value}";
        }
        else
        {
            textValue = $"<color=#b93333>{value}";
        }

        TotalEarnings.text = $"Total earnings: {textValue} {Currency}</color>";
    }

    public void Exit() 
    {
        Application.Quit();
    }

    public void Block(bool value)
    {
        if (Blockade.gameObject.activeSelf != value)
        {
            Blockade.gameObject.SetActive(value);
        }
    }
}
