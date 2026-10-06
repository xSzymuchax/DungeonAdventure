using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePlate : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Rigidbody body;
    [SerializeField] float rise = 2.4f;
    [SerializeField] float duration = 0.75f;
    [SerializeField] float launchSpeed = 6f;
    [SerializeField] float gravityScale = 3f;
    [SerializeField] float life = 12f;

    Quaternion textOffset = Quaternion.identity;

    public void Launch(int amount, DamageType type)
    {
        if (label != null)
        {
            label.text = amount.ToString();
            label.color = DamageTypes.ColorOf(type);
            Transform text = label.canvas != null ? label.canvas.transform : label.transform;
            textOffset = text.localRotation;
            FaceCamera();
        }

        if (body == null)
            body = GetComponent<Rigidbody>();

        body.isKinematic = true;
        if (life > 0f)
            Destroy(gameObject, life);
        StartCoroutine(Hop());
    }

    IEnumerator Hop()
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            transform.position = start + Vector3.up * (rise * t);
            float pop = t < 0.18f ? Mathf.Lerp(1.45f, 1f, t / 0.18f) : 1f;
            transform.localScale = Vector3.one * pop;
            FaceCamera();
            yield return null;
        }

        transform.localScale = Vector3.one;
        Shove();
    }

    void Shove()
    {
        Vector3 away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        if (away.sqrMagnitude < 0.01f)
            away = Vector3.forward;
        away.Normalize();

        body.isKinematic = false;
        body.velocity = away * launchSpeed;
        body.angularVelocity = Random.insideUnitSphere * 8f;
    }

    void FixedUpdate()
    {
        if (body == null || body.isKinematic || gravityScale <= 1f)
            return;

        body.AddForce(Physics.gravity * (gravityScale - 1f), ForceMode.Acceleration);
    }

    void FaceCamera()
    {
        Camera cam = GameController.Instance != null ? GameController.Instance.playerCamera : null;
        if (cam == null)
            return;

        transform.rotation = cam.transform.rotation * Quaternion.Inverse(textOffset);
    }
}
