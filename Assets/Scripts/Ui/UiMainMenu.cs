using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiMainMenu : MonoBehaviour
{
    public UiHoldings UiHoldings;
    public UiNewHolding AddingNew;
    public HoldingsManager HoldingsManager;
    public Image Blockade;

    public void Refresh()
    {
        Block(true);
        StartCoroutine(ForceDownloadLatestHolding());
    }

    private IEnumerator ForceDownloadLatestHolding()
    {
        yield return new WaitForEndOfFrame();

        HoldingsManager.ForceDownloadLatestHolding();
        yield return new WaitForEndOfFrame();

        UiHoldings.CreatedHoldings = false;
        Block(false);
        yield return new WaitForEndOfFrame();
    }

    private void Block(bool value)
    {
        if (Blockade.gameObject.activeSelf != value)
        {
            Blockade.gameObject.SetActive(value);
        }
    }
}
