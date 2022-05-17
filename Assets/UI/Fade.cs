using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class Fade : EventTrigger
{
    private CanvasGroup canvasGroup;
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
