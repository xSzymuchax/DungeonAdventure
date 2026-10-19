using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceDisplay : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] TMP_Text levelLabel;

    PlayerCharacter player;

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    void OnDisable()
    {
        if (GameController.Instance != null)
            GameController.Instance.PlayerReady -= BindPlayer;
        UnbindPlayer();
    }

    System.Collections.IEnumerator BindWhenReady()
    {
        while (GameController.Instance == null)
            yield return null;

        GameController.Instance.PlayerReady += BindPlayer;
        if (GameController.Instance.Player != null)
            BindPlayer(GameController.Instance.Player);
    }

    void BindPlayer(PlayerCharacter current)
    {
        if (player == current)
            return;

        UnbindPlayer();
        player = current;
        if (player == null)
            return;

        player.ProgressChanged += Refresh;
        Refresh();
    }

    void UnbindPlayer()
    {
        if (player == null)
            return;

        player.ProgressChanged -= Refresh;
        player = null;
    }

    void Refresh()
    {
        if (bar != null)
            bar.fillAmount = Fill();
        if (levelLabel != null && player != null)
            levelLabel.text = player.Level.ToString();
    }

    float Fill()
    {
        if (player == null)
            return 0f;
        if (player.Level >= Consts.MAX_PLAYER_LEVEL || Consts.EXPERIENCE_PER_LEVEL <= 0)
            return 1f;
        return Mathf.Clamp01(player.Experience / (float)Consts.EXPERIENCE_PER_LEVEL);
    }
}
