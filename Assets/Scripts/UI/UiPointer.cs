using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UiPointer
{
    static readonly List<RaycastResult> hits = new List<RaycastResult>();
    static PointerEventData data;

    public static bool Over()
    {
        if (EventSystem.current == null)
            return false;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Canceled)
                    continue;
                if (Covers(touch.position))
                    return true;
            }

            return false;
        }

        return Covers(Input.mousePosition);
    }

    public static bool Covers(Vector2 screen)
    {
        if (EventSystem.current == null)
            return false;
        if (data == null)
            data = new PointerEventData(EventSystem.current);
        data.Reset();
        data.position = screen;
        hits.Clear();
        EventSystem.current.RaycastAll(data, hits);
        return hits.Count > 0;
    }
}
