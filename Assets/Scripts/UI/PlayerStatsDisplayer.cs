using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsDisplayer : MonoBehaviour
{
    [SerializeField] float barAnimationDuration = 0.2f;
    [SerializeField] float barBackgroundDelay = 0.35f;

    [SerializeField] Image healthBar;
    [SerializeField] Image healthBarBackground;
    [SerializeField] Image manaBar;
    [SerializeField] Image manaBarBackground;
    [SerializeField] Image saturationBar;
    [SerializeField] Image saturationBarBackground;
    [SerializeField] Image hydrationBar;
    [SerializeField] Image hydrationBarBackground;
    [SerializeField] Image sanityBar;
    [SerializeField] Image sanityBarBackground;

    [SerializeField] TMP_Text maxHealthText;
    [SerializeField] TMP_Text manaText;
    [SerializeField] TMP_Text strengthText;
    [SerializeField] TMP_Text knowledgeText;

    private PlayerCharacter player;
    private Coroutine healthFrontRoutine;
    private Coroutine healthBackRoutine;
    private Coroutine manaFrontRoutine;
    private Coroutine manaBackRoutine;
    private Coroutine saturationFrontRoutine;
    private Coroutine saturationBackRoutine;
    private Coroutine hydrationFrontRoutine;
    private Coroutine hydrationBackRoutine;
    private Coroutine sanityFrontRoutine;
    private Coroutine sanityBackRoutine;

    private void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (GameController.Instance != null)
            GameController.Instance.PlayerReady -= BindPlayer;
        UnbindPlayer();
    }

    private IEnumerator BindWhenReady()
    {
        while (GameController.Instance == null)
            yield return null;

        GameController.Instance.PlayerReady += BindPlayer;
        if (GameController.Instance.Player != null)
            BindPlayer(GameController.Instance.Player);
    }

    private void BindPlayer(PlayerCharacter current)
    {
        if (player == current)
            return;

        UnbindPlayer();
        player = current;
        if (player == null)
            return;

        player.HealthChanged += OnHealthChanged;
        player.ManaChanged += OnManaChanged;
        player.SatietyChanged += OnSatietyChanged;
        player.HydrationChanged += OnHydrationChanged;
        player.SanityChanged += OnSanityChanged;
        SnapAll();
    }

    private void UnbindPlayer()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
            player.ManaChanged -= OnManaChanged;
            player.SatietyChanged -= OnSatietyChanged;
            player.HydrationChanged -= OnHydrationChanged;
            player.SanityChanged -= OnSanityChanged;
        }

        player = null;
        StopBarRoutines();
    }

    private void OnHealthChanged()
    {
        RefreshTexts();
        PlayBar(healthBar, healthBarBackground, HealthFill(), ref healthFrontRoutine, ref healthBackRoutine);
    }

    private void OnManaChanged()
    {
        RefreshTexts();
        PlayBar(manaBar, manaBarBackground, ManaFill(), ref manaFrontRoutine, ref manaBackRoutine);
    }

    private void OnSatietyChanged()
    {
        PlayBar(saturationBar, saturationBarBackground, SatietyFill(), ref saturationFrontRoutine, ref saturationBackRoutine);
    }

    private void OnHydrationChanged()
    {
        PlayBar(hydrationBar, hydrationBarBackground, HydrationFill(), ref hydrationFrontRoutine, ref hydrationBackRoutine);
    }

    private void OnSanityChanged()
    {
        PlayBar(sanityBar, sanityBarBackground, SanityFill(), ref sanityFrontRoutine, ref sanityBackRoutine);
    }

    private void SnapAll()
    {
        float health = HealthFill();
        float mana = ManaFill();
        SetFill(healthBar, health);
        SetFill(healthBarBackground, health);
        SetFill(manaBar, mana);
        SetFill(manaBarBackground, mana);

        float saturation = SatietyFill();
        float hydration = HydrationFill();
        float sanity = SanityFill();
        SetFill(saturationBar, saturation);
        SetFill(saturationBarBackground, saturation);
        SetFill(hydrationBar, hydration);
        SetFill(hydrationBarBackground, hydration);
        SetFill(sanityBar, sanity);
        SetFill(sanityBarBackground, sanity);
        RefreshTexts();
    }

    private void RefreshTexts()
    {
        if (player == null)
            return;

        CharacterStats stats = player.Stats;
        PlayerStats playerStats = player.PlayerStats;
        SetText(maxHealthText, stats != null ? stats.MaxHealth.ToString() : "0");
        SetText(manaText, stats != null ? stats.MaxMana.ToString("0.##") : "0");
        SetText(strengthText, playerStats != null ? playerStats.Strength.ToString() : "0");
        SetText(knowledgeText, playerStats != null ? playerStats.Knowledge.ToString() : "0");
    }

    private float HealthFill()
    {
        if (player == null || player.Stats == null)
            return 0f;
        return Ratio(player.Health, player.Stats.MaxHealth);
    }

    private float ManaFill()
    {
        if (player == null || player.Stats == null)
            return 0f;
        return Ratio((float)player.Mana, (float)player.Stats.MaxMana);
    }

    private float SatietyFill()
    {
        PlayerStats playerStats = player != null ? player.PlayerStats : null;
        return Ratio(player != null ? player.Satiety : 0, playerStats != null ? playerStats.MaxSatiety : 0);
    }

    private float HydrationFill()
    {
        PlayerStats playerStats = player != null ? player.PlayerStats : null;
        return Ratio(player != null ? player.Hydration : 0, playerStats != null ? playerStats.MaxHydration : 0);
    }

    private float SanityFill()
    {
        PlayerStats playerStats = player != null ? player.PlayerStats : null;
        return Ratio(player != null ? player.Sanity : 0, playerStats != null ? playerStats.MaxSanity : 0);
    }

    private void PlayBar(Image front, Image back, float target, ref Coroutine frontRoutine, ref Coroutine backRoutine)
    {
        if (frontRoutine != null)
            StopCoroutine(frontRoutine);
        frontRoutine = StartCoroutine(AnimateFill(front, target));

        if (backRoutine != null)
            StopCoroutine(backRoutine);

        if (back == null)
            return;

        if (back.fillAmount <= target)
        {
            back.fillAmount = target;
            backRoutine = null;
            return;
        }

        backRoutine = StartCoroutine(AnimateFillDelayed(back, target));
    }

    private IEnumerator AnimateFillDelayed(Image image, float target)
    {
        if (barBackgroundDelay > 0f)
            yield return new WaitForSeconds(barBackgroundDelay);
        yield return AnimateFill(image, target);
    }

    private IEnumerator AnimateFill(Image image, float target)
    {
        if (image == null)
            yield break;

        float start = image.fillAmount;
        if (barAnimationDuration <= 0f || Mathf.Approximately(start, target))
        {
            image.fillAmount = target;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < barAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutLog(elapsed / barAnimationDuration);
            image.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }

        image.fillAmount = target;
    }

    private void StopBarRoutines()
    {
        if (healthFrontRoutine != null)
            StopCoroutine(healthFrontRoutine);
        if (healthBackRoutine != null)
            StopCoroutine(healthBackRoutine);
        if (manaFrontRoutine != null)
            StopCoroutine(manaFrontRoutine);
        if (manaBackRoutine != null)
            StopCoroutine(manaBackRoutine);
        if (saturationFrontRoutine != null)
            StopCoroutine(saturationFrontRoutine);
        if (saturationBackRoutine != null)
            StopCoroutine(saturationBackRoutine);
        if (hydrationFrontRoutine != null)
            StopCoroutine(hydrationFrontRoutine);
        if (hydrationBackRoutine != null)
            StopCoroutine(hydrationBackRoutine);
        if (sanityFrontRoutine != null)
            StopCoroutine(sanityFrontRoutine);
        if (sanityBackRoutine != null)
            StopCoroutine(sanityBackRoutine);
        healthFrontRoutine = null;
        healthBackRoutine = null;
        manaFrontRoutine = null;
        manaBackRoutine = null;
        saturationFrontRoutine = null;
        saturationBackRoutine = null;
        hydrationFrontRoutine = null;
        hydrationBackRoutine = null;
        sanityFrontRoutine = null;
        sanityBackRoutine = null;
    }

    private static float EaseOutLog(float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Log10(1f + 9f * t);
    }

    private static float Ratio(float current, float max)
    {
        return max <= 0f ? 0f : Mathf.Clamp01(current / max);
    }

    private static void SetFill(Image image, float amount)
    {
        if (image == null)
            return;
        image.fillAmount = amount;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text == null)
            return;
        text.text = value;
    }
}
