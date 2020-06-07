using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiConfirmation : MonoBehaviour
{
    public UiManager UiManager;
    private Action OnYes;
    private Action OnNo;
    public TMP_Text Question;

    public void Yes()
    {
        OnYes.Invoke();
        Hide();
    }

    public void No()
    {
        OnNo.Invoke();
        Hide();
    }

    public void Show(Action yesAction, Action noAction, string question)
    {
        UiManager.Block(true);
        Question.text = question;
        OnYes = yesAction;
        OnNo = noAction;
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        UiManager.Block(false);
        gameObject.SetActive(false);
    }
}
