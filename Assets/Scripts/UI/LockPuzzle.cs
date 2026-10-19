using UnityEngine;
using UnityEngine.UI;

public class LockPuzzle : MonoBehaviour
{
    Padlock padlock;
    GameObject root;

    public bool IsOpen => root != null && root.activeSelf;

    public void Ensure(Transform parent)
    {
        if (root != null || parent == null)
            return;

        root = new GameObject("LockPuzzle", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);
        RectTransform panel = root.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(280f, 140f);
        Image image = root.GetComponent<Image>();
        image.color = new Color(0.12f, 0.1f, 0.08f, 0.92f);

        GameObject buttonObject = new GameObject("Open", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(root.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(180f, 48f);
        Image buttonImage = buttonObject.GetComponent<Image>();
        Color color = new Color(0.75f, 0.62f, 0.28f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        buttonImage.color = color;
        button.onClick.AddListener(Press);
        root.SetActive(false);
    }

    public void Show(Padlock target)
    {
        padlock = target;
        if (root != null)
            root.SetActive(padlock != null);
    }

    public void Hide()
    {
        padlock = null;
        if (root != null)
            root.SetActive(false);
    }

    void Press()
    {
        if (padlock != null)
            padlock.Solve();
        Hide();
    }
}
