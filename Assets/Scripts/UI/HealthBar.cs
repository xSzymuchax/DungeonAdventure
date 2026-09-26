using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image front;
    [SerializeField] Image background;
    [SerializeField] float barAnimationDuration = 0.2f;
    [SerializeField] float barBackgroundDelay = 0.35f;

    IDamagable target;
    Coroutine frontRoutine;
    Coroutine backRoutine;

    void Start()
    {
        target = GetComponentInParent<IDamagable>();
        if (target == null)
            return;

        target.HealthChanged += Refresh;
        Snap();
    }

    void OnDestroy()
    {
        if (target != null)
            target.HealthChanged -= Refresh;
    }

    void Snap()
    {
        float amount = Fill();
        SetFill(front, amount);
        SetFill(background, amount);
        gameObject.SetActive(!target.IsDead);
    }

    void Refresh()
    {
        if (target == null)
            return;

        if (target.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        float amount = Fill();
        if (frontRoutine != null)
            StopCoroutine(frontRoutine);
        frontRoutine = StartCoroutine(AnimateFill(front, amount));

        if (backRoutine != null)
            StopCoroutine(backRoutine);

        if (background == null)
            return;

        if (background.fillAmount <= amount)
        {
            background.fillAmount = amount;
            backRoutine = null;
            return;
        }

        backRoutine = StartCoroutine(AnimateFillDelayed(background, amount));
    }

    float Fill()
    {
        if (target == null || target.MaxHealth <= 0)
            return 0f;
        return Mathf.Clamp01(target.Health / (float)target.MaxHealth);
    }

    IEnumerator AnimateFillDelayed(Image image, float targetFill)
    {
        if (barBackgroundDelay > 0f)
            yield return new WaitForSeconds(barBackgroundDelay);
        yield return AnimateFill(image, targetFill);
    }

    IEnumerator AnimateFill(Image image, float targetFill)
    {
        if (image == null)
            yield break;

        float start = image.fillAmount;
        if (barAnimationDuration <= 0f || Mathf.Approximately(start, targetFill))
        {
            image.fillAmount = targetFill;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < barAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / barAnimationDuration);
            image.fillAmount = Mathf.Lerp(start, targetFill, Mathf.Log10(1f + 9f * t));
            yield return null;
        }

        image.fillAmount = targetFill;
    }

    static void SetFill(Image image, float amount)
    {
        if (image != null)
            image.fillAmount = amount;
    }
}
