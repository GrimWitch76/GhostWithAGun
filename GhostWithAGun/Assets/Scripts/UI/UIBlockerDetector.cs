using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIBlockerDetector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Or your preferred input
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                Debug.Log("Clicked on/blocked by: " + results[0].gameObject.name);
                // You can iterate through 'results' to see all overlapping elements
            }
        }
    }
}