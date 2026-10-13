using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationWindow : MonoBehaviour
{
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text body;
    [SerializeField] Image icon;
    [SerializeField] Button closeButton;
    [SerializeField] RectTransform tokenList;
    [SerializeField] GameObject tokenDisplayPrefab;

    readonly List<GameObject> spawned = new();
    IToken current;
    IReadOnlyList<IToken> listed;
    bool showingPage;
    bool watching;
    InformationWindow tokenDetail;
    static bool spawningDetail;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (spawningDetail)
        {
            if (tokenList != null)
                tokenList.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        if (tokenDetail == null)
        {
            spawningDetail = true;
            GameObject copy = Instantiate(gameObject, transform.parent);
            spawningDetail = false;
            copy.name = "TokenWindow";
            tokenDetail = copy.GetComponent<InformationWindow>();
            RectTransform rect = copy.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition += new Vector2(48f, -48f);
            copy.transform.SetAsLastSibling();
        }

        if (current == null && !showingPage)
            gameObject.SetActive(false);
    }

    public void ShowSubject(string subjectTitle, string subjectBody, Sprite image, IReadOnlyList<IToken> tokens)
    {
        current = null;
        showingPage = true;
        listed = tokens;
        gameObject.SetActive(true);
        Apply(subjectTitle, subjectBody, image, tokens);
    }

    public void Toggle(IToken token)
    {
        if (token == null)
            return;

        if (current == token && gameObject.activeSelf)
        {
            Hide();
            return;
        }

        current = token;
        showingPage = false;
        listed = null;
        gameObject.SetActive(true);
        Apply(token.Name(), token.Description(), token.TokenImage(), null);
    }

    public void Follow(IReadOnlyList<IToken> tokens)
    {
        if (tokenDetail != null)
            tokenDetail.RefreshIfPresent(tokens);

        if (current == null || !gameObject.activeSelf)
            return;

        if (tokens != null)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == current)
                {
                    Apply(current.Name(), current.Description(), current.TokenImage(), null);
                    return;
                }
            }
        }

        Hide();
    }

    void RefreshIfPresent(IReadOnlyList<IToken> tokens)
    {
        if (current == null || !gameObject.activeSelf || tokens == null)
            return;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] != current)
                continue;

            Apply(current.Name(), current.Description(), current.TokenImage(), null);
            return;
        }

        if (watching)
            Hide();
    }

    public void Hide()
    {
        current = null;
        listed = null;
        showingPage = false;
        ClearTokens();
        gameObject.SetActive(false);
    }

    void Apply(string subjectTitle, string subjectBody, Sprite image, IReadOnlyList<IToken> tokens)
    {
        if (title != null)
            title.text = subjectTitle ?? "";
        if (body != null)
            body.text = subjectBody ?? "";
        if (icon != null)
        {
            icon.sprite = image;
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.enabled = image != null;
        }

        RebuildTokens(tokens);
    }

    void RebuildTokens(IReadOnlyList<IToken> tokens)
    {
        ClearTokens();
        if (tokenList != null)
            tokenList.gameObject.SetActive(tokens != null && tokens.Count > 0);

        if (tokenList == null || tokenDisplayPrefab == null || tokens == null)
            return;

        for (int i = 0; i < tokens.Count; i++)
        {
            IToken token = tokens[i];
            if (token == null)
                continue;

            GameObject slot = Instantiate(tokenDisplayPrefab, tokenList);
            slot.name = tokenDisplayPrefab.name;
            Image image = slot.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = token.TokenImage();
                image.preserveAspect = true;
            }

            int index = i;
            Button button = slot.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OpenListed(index));
            spawned.Add(slot);
        }
    }

    void OpenListed(int index)
    {
        if (listed == null || index < 0 || index >= listed.Count)
            return;

        ShowToken(listed[index], false);
    }

    public void ShowToken(IToken token, bool watch)
    {
        if (token == null)
            return;

        if (tokenDetail == null)
        {
            Toggle(token);
            return;
        }

        tokenDetail.watching = watch;
        tokenDetail.Toggle(token);
        tokenDetail.transform.SetAsLastSibling();
    }

    void ClearTokens()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }

        spawned.Clear();
    }

}
