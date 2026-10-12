using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeWorkbenchPanel : MonoBehaviour
{
    public Image itemSlot;
    public Image[] resourceSlots;
    public Image resultSlot;
    public ItemDisplayer itemView;
    public ItemDisplayer[] resourceViews;
    public ItemDisplayer resultView;
    public ItemPreview previewPrefab;
    public TMP_Text buffs;
    public Button confirm;
    public Button close;

    public bool IsOpen => gameObject.activeInHierarchy;
    public CraftBench Bench => bench;

    Workbench bench;
    PlayerCharacter player;
    bool wired;
    bool allowActive;
    bool awaitingPick;

    void Awake()
    {
        if (!allowActive)
            gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Wire();
        if (confirm != null)
        {
            confirm.onClick.RemoveListener(Confirm);
            confirm.onClick.AddListener(Confirm);
        }

        if (close != null)
        {
            close.onClick.RemoveListener(Close);
            close.onClick.AddListener(Close);
        }

        Refresh();
    }

    void OnDisable()
    {
        CancelPick();
        ClearViews();
    }

    void OnDestroy()
    {
        Release(itemView);
        Release(resultView);
        if (resourceViews == null)
            return;
        for (int i = 0; i < resourceViews.Length; i++)
            Release(resourceViews[i]);
    }

    public void Open(PlayerCharacter character, Workbench source)
    {
        player = character;
        bench = source;
        allowActive = true;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        bench = null;
        player = null;
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        GameObject model = GameController.Instance != null ? GameController.Instance.itemModel : null;
        Show(itemView, bench != null ? bench.Subject : null, model);
        Show(resultView, bench != null ? bench.ShownResult() : null, model);
        if (resourceViews != null)
        {
            for (int i = 0; i < resourceViews.Length; i++)
                Show(resourceViews[i], bench != null ? bench.ResourceAt(i) : null, model);
        }

        if (buffs != null)
            buffs.text = bench != null ? bench.Preview() : "";
    }

    void Show(ItemDisplayer view, Item item, GameObject model)
    {
        if (view != null)
            view.Show(item, false, model, previewPrefab, true);
    }

    void ClearViews()
    {
        if (itemView != null)
            itemView.Clear();
        if (resultView != null)
            resultView.Clear();
        if (resourceViews == null)
            return;
        for (int i = 0; i < resourceViews.Length; i++)
        {
            if (resourceViews[i] != null)
                resourceViews[i].Clear();
        }
    }

    static void Release(ItemDisplayer view)
    {
        if (view != null)
            view.Release();
    }

    void Wire()
    {
        if (wired)
            return;

        wired = true;
        Bind(itemSlot, OnSubject);
        if (resourceSlots != null)
        {
            for (int i = 0; i < resourceSlots.Length; i++)
            {
                int index = i;
                Bind(resourceSlots[i], () => OnResource(index));
            }
        }

        Bind(resultSlot, TakeResult);
    }

    void OnSubject()
    {
        if (bench == null || player == null)
            return;
        if (bench.Subject != null)
        {
            CancelPick();
            bench.ReturnSubject(player.Inventory);
            return;
        }

        Ask(item => bench != null && player != null && bench.TryPlaceSubject(player.Inventory, item));
    }

    void OnResource(int index)
    {
        if (bench == null || player == null)
            return;
        if (bench.ResourceAt(index) != null)
        {
            CancelPick();
            bench.ReturnResource(player.Inventory, index);
            return;
        }

        Ask(item => bench != null && player != null && bench.TryPlaceResource(player.Inventory, index, item));
    }

    void Ask(Func<Item, bool> place)
    {
        InventoryDisplay inventory = Inventory();
        if (inventory == null)
            return;

        inventory.BeginAssign(item =>
        {
            awaitingPick = false;
            if (place(item))
                CloseInventory();
            else
                Ask(place);
        });
        awaitingPick = true;
        OpenInventory();
    }

    void CancelPick()
    {
        if (!awaitingPick)
            return;

        awaitingPick = false;
        InventoryDisplay inventory = Inventory();
        if (inventory != null)
            inventory.EndAssign();
        CloseInventory();
    }

    void OpenInventory()
    {
        ToggleCard card = Card();
        if (card != null)
            card.Open();
    }

    void CloseInventory()
    {
        ToggleCard card = Card();
        if (card != null)
            card.Close();
    }

    InventoryDisplay Inventory()
    {
        return GameController.Instance != null ? GameController.Instance.inventoryDisplay : null;
    }

    ToggleCard Card()
    {
        InventoryDisplay inventory = Inventory();
        return inventory != null ? inventory.GetComponent<ToggleCard>() : null;
    }

    void TakeResult()
    {
        if (bench != null && player != null)
            bench.TakeResult(player.Inventory);
    }

    void Confirm()
    {
        if (bench == null || player == null || GameController.Instance == null)
            return;

        GameController.Instance.BeginWorkbenchConfirm(player, bench);
    }

    static void Bind(Image image, UnityEngine.Events.UnityAction action)
    {
        if (image == null)
            return;

        Color color = image.color;
        Button button = image.GetComponent<Button>();
        image.color = color;
        if (button != null)
            button.onClick.AddListener(action);
    }
}
