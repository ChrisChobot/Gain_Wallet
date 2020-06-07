using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UiHoldings : MonoBehaviour
{
    public HoldingsManager HoldingsManager;
    public UiManager UiManager;
    public UiHolding HoldingPrefab;
    public Transform HoldingList;
    private List<UiHolding> DisplayedHoldings = new List<UiHolding>();
    public bool CreatedHoldings;
    public Scrollbar Scrollbar;

    public void Show(bool value)
    {
        UiManager.IsShowWindowOpen = value;
        gameObject.SetActive(value);
        if (value && !CreatedHoldings)
        {
            CreateHoldingList();
        }
    }

    private void CreateHoldingList()
    {
        ClearHoldingList();
        var holdings = HoldingsManager.GetUserHoldings();

        foreach (var item in holdings)
        {
            UiHolding holding = Instantiate(HoldingPrefab, HoldingList);
            holding.Init(UiManager, item.Key, item.Value);
            DisplayedHoldings.Add(holding);
        }

        DisplayedHoldings.OrderBy(x => x.HoldingName);
        StartCoroutine(AdjustListSize());
        CreatedHoldings = true;
    }

    private void ClearHoldingList()
    {
        for (int i = DisplayedHoldings.Count - 1; i >= 0; i--)
        {
            Destroy(DisplayedHoldings[i].gameObject);
        }
        DisplayedHoldings.Clear();
    }

    private IEnumerator AdjustListSize()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        RectTransform rectTransform = HoldingList as RectTransform;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x,150* DisplayedHoldings.Count);
        Scrollbar.value = 1;
    }
}
