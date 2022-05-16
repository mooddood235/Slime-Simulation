using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Runtime.InteropServices;

[RequireComponent(typeof(TMP_InputField))]

public class SliderField : EventTrigger
{
    private TMP_InputField inputField;
    private float value;
    private bool drag;
    private Vector2Int cursorPos;

    private void Awake() {
        inputField = GetComponent<TMP_InputField>();
        value = float.Parse(inputField.text);
        drag = false;
    }
    private void Update() {
        if (Input.GetMouseButtonUp(1)){
            drag = false;
            Cursor.visible = true; 
        }

        if (drag){
            float acceleration = Input.GetAxis("Mouse X");
            
            float delta = 0;
            if (acceleration > 0) delta = 1;
            else if (acceleration < 0) delta = -1;

            delta *= Mathf.Max(1, Mathf.Pow(acceleration * 10, 2));

            if (float.TryParse(inputField.text, out value)){
                inputField.text = Math.Round((value + delta / 100f), 2).ToString();
            }

            SetCursorPos(cursorPos.x, cursorPos.y);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right){
            drag = true;
            GetCursorPos(out cursorPos);
            Cursor.visible = false;
        }
    }

    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Vector2Int pos);
}
