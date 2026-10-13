using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokensDisplay : MonoBehaviour
{
    [SerializeField] InformationWindow informationWindow;
    [SerializeField] GameObject tokenDisplayPrefab;

    public InformationWindow Window => informationWindow;

    PlayerCharacter player;
    readonly List<Image> slots = new();

    public void Bind(PlayerCharacter player)
    {
        if (this.player != null)
            this.player.TokensChanged -= Refresh;

        this.player = player;
        if (player != null)
            player.TokensChanged += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (player != null)
            player.TokensChanged -= Refresh;
    }

    void Refresh()
    {
        IReadOnlyList<IToken> tokens = player != null ? player.ActiveTokens : null;
        int count = tokens != null ? tokens.Count : 0;

        while (slots.Count < count)
        {
            Image slot = CreateSlot();
            if (slot == null)
                break;
            slots.Add(slot);
        }

        while (slots.Count > count)
        {
            int last = slots.Count - 1;
            Destroy(slots[last].gameObject);
            slots.RemoveAt(last);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            IToken token = tokens[i];
            Image slot = slots[i];
            slot.sprite = token.TokenImage();
            slot.preserveAspect = true;
            int index = i;
            Button button = slot.GetComponent<Button>();
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Select(index));
        }

        if (count > 0)
            gameObject.SetActive(true);

        if (informationWindow != null)
            informationWindow.Follow(tokens);
    }

    Image CreateSlot()
    {
        if (tokenDisplayPrefab == null)
            return null;

        GameObject slot = Instantiate(tokenDisplayPrefab, transform);
        slot.name = tokenDisplayPrefab.name;
        return slot.GetComponent<Image>();
    }

    void Select(int index)
    {
        if (informationWindow == null || player == null)
            return;

        IReadOnlyList<IToken> tokens = player.ActiveTokens;
        if (index < 0 || index >= tokens.Count)
            return;

        informationWindow.ShowToken(tokens[index], true);
    }
}
