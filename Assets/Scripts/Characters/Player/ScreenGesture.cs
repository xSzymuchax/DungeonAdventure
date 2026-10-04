using UnityEngine;

public static class ScreenGesture
{
    public static float MouseSlop = 8f;

    static int frame = -1;
    static int finger = -1;
    static bool fromUi;
    static bool moved;
    static bool releaseOverUi;
    static bool began;
    static bool held;
    static bool ended;
    static bool touch;
    static Vector2 position;
    static Vector2 previous;
    static Vector2 origin;

    public static bool Began => began;
    public static bool WorldHeld => held && !fromUi && !UiPointer.Covers(position);
    public static bool Tap => ended && !fromUi && !moved && !releaseOverUi;
    public static bool Panned => moved && !fromUi;
    public static Vector2 Position => position;
    public static Vector2 Delta => position - previous;

    public static void Poll()
    {
        if (frame == Time.frameCount)
            return;

        frame = Time.frameCount;
        began = false;
        held = false;
        ended = false;

        if (Input.touchCount > 0)
        {
            touch = true;
            ReadTouch();
            return;
        }

        touch = false;
        finger = -1;
        ReadMouse();
    }

    static float Slop => touch ? Mathf.Max(MouseSlop * 3f, Mathf.Min(Screen.width, Screen.height) * 0.03f) : MouseSlop;

    static void ReadTouch()
    {
        bool found = false;
        Touch current = default;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch candidate = Input.GetTouch(i);
            if (candidate.fingerId != finger)
                continue;

            current = candidate;
            found = true;
            break;
        }

        if (!found)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch candidate = Input.GetTouch(i);
                if (candidate.phase != TouchPhase.Began)
                    continue;

                Begin(candidate.fingerId, candidate.position);
                return;
            }

            return;
        }

        previous = position;
        position = current.position;
        if (current.phase == TouchPhase.Ended)
        {
            Finish(false);
            return;
        }

        if (current.phase == TouchPhase.Canceled)
        {
            Finish(true);
            return;
        }

        held = true;
        NoteMove();
    }

    static void ReadMouse()
    {
        Vector2 point = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
            Begin(-1, point);

        if (Input.GetMouseButton(0))
        {
            previous = position;
            position = point;
            held = true;
            NoteMove();
        }

        if (Input.GetMouseButtonUp(0))
        {
            previous = position;
            position = point;
            Finish(false);
        }
    }

    static void Begin(int id, Vector2 point)
    {
        finger = id;
        fromUi = UiPointer.Covers(point);
        moved = false;
        releaseOverUi = false;
        origin = point;
        previous = point;
        position = point;
        began = true;
        held = true;
        ended = false;
    }

    static void NoteMove()
    {
        if (fromUi || moved)
            return;
        if ((position - origin).sqrMagnitude >= Slop * Slop)
            moved = true;
    }

    static void Finish(bool lost)
    {
        releaseOverUi = lost || UiPointer.Covers(position);
        ended = true;
        held = false;
        finger = -1;
    }
}
