using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDebugClickLogger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.Log("[UI Debug] No UI element hit by click.");
            }
            else
            {
                Debug.Log($"[UI Debug] {results.Count} UI elements hit (topmost first):");
                for (int i = 0; i < results.Count; i++)
                {
                    var r = results[i];
                    Debug.Log($"[{i}] {r.gameObject.name} (Layer: {r.gameObject.layer})");
                }
            }
        }
    }
}