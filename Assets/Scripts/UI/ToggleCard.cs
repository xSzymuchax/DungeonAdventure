using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCard : MonoBehaviour
{
    [SerializeField] float duration = 0.25f;

    private RectTransform panel;
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen;
    private Coroutine animationRoutine;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        closedPosition = panel.anchoredPosition;
        float direction = panel.anchorMin.x >= 1f ? -1f : 1f;
        openPosition = closedPosition + new Vector2(direction * panel.rect.width, 0f);

        Transform handle = transform.Find("Toggle");
        if (handle == null)
            handle = transform.Find("ToggleButton");
        if (handle == null)
            return;

        Button button = handle.GetComponent<Button>();
        if (button == null)
            button = handle.gameObject.AddComponent<Button>();
        button.onClick.AddListener(Toggle);
    }

    public void Open()
    {
        if (isOpen)
            return;
        isOpen = true;
        Play(openPosition);
    }

    public void Close()
    {
        if (!isOpen)
            return;
        isOpen = false;
        Play(closedPosition);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        Play(isOpen ? openPosition : closedPosition);
    }

    void Play(Vector2 target)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(ToggleAnimation(target));
    }

    private IEnumerator ToggleAnimation(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : elapsed / duration;
            panel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target;
        animationRoutine = null;
    }
}
