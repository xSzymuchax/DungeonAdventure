using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunecraftWorkbenchPanel : MonoBehaviour
{
    public Image[] slots;
    public Image resultSlot;
    public ItemDisplayer[] slotViews;
    public ItemDisplayer resultView;
    public TMP_Text resultName;
    public ItemPreview previewPrefab;
    public Button confirm;
    public Button close;

    public bool IsOpen => gameObject.activeInHierarchy;
    public CraftBench Bench => bench;

    RunecraftWorkbench bench;
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
        if (slotViews != null)
        {
            for (int i = 0; i < slotViews.Length; i++)
                Release(slotViews[i]);
        }

        Release(resultView);
    }

    public void Open(PlayerCharacter character, RunecraftWorkbench source)
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
        if (slotViews != null)
        {
            for (int i = 0; i < slotViews.Length; i++)
                Show(slotViews[i], bench != null ? bench.SlotAt(i) : null, model);
        }

        Item shown = bench != null ? bench.ShownResult() : null;
        Show(resultView, shown, model);
        if (resultName != null)
            resultName.text = shown != null ? shown.DisplayName : "";
    }

    void Show(ItemDisplayer view, Item item, GameObject model)
    {
        if (view != null)
            view.Show(item, false, model, previewPrefab, true);
    }

    void ClearViews()
    {
        if (slotViews != null)
        {
            for (int i = 0; i < slotViews.Length; i++)
            {
                if (slotViews[i] != null)
                    slotViews[i].Clear();
            }
        }

        if (resultView != null)
            resultView.Clear();
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
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            Bind(slots[i], () => OnSlot(index));
        }

        Bind(resultSlot, TakeResult);
    }

    void OnSlot(int index)
    {
        if (bench == null || player == null)
            return;
        if (bench.SlotAt(index) != null)
        {
            CancelPick();
            bench.ReturnSlot(player.Inventory, index);
            return;
        }

        Ask(item => bench != null && player != null && bench.TryPlaceAt(player.Inventory, index, item));
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
        if (button == null)
            button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        image.color = color;
        button.onClick.AddListener(action);
    }
}
