using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class Fade : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Update() {
        if (Show()){
            canvasGroup.alpha = 1;
        }
        else{
            canvasGroup.alpha = 0;
        }
    }
    private bool Show(){
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> rayCastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, rayCastResults);

        foreach (RaycastResult rayCastResult in rayCastResults){
            if (rayCastResult.gameObject == this.gameObject){
                return true;
            }
        }
        return false;
    }
}
