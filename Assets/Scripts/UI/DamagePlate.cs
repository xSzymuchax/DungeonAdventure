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
    [SerializeField] float life = 12f;

    Transform face;
    // Quaternion faceRest;

    public void Launch(int amount, DamageType type, Collider[] ignore)
    {
        if (label != null)
        {
            label.text = amount.ToString();
            label.color = DamageTypes.ColorOf(type);
            face = label.canvas != null ? label.canvas.transform : label.transform;
            // faceRest = face.localRotation;
            this.transform.localRotation = Quaternion.identity;
        }

        if (body == null)
            body = GetComponent<Rigidbody>();

        body.isKinematic = true;
        IgnoreVictim(ignore);
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
        // if (face != null)
        //     face.localRotation = faceRest;

        Vector3 away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        if (away.sqrMagnitude < 0.01f)
            away = Vector3.forward;
        away.Normalize();

        transform.rotation = Quaternion.Euler(Random.Range(-25f, 25f), Random.Range(0f, 360f), Random.Range(-25f, 25f));
        body.isKinematic = false;
        body.velocity = away * launchSpeed;
        body.angularVelocity = Random.insideUnitSphere * 8f;
    }

    void IgnoreVictim(Collider[] ignore)
    {
        Collider plate = GetComponent<Collider>();
        if (plate == null || ignore == null)
            return;

        for (int i = 0; i < ignore.Length; i++)
        {
            if (ignore[i] != null && ignore[i] != plate)
                Physics.IgnoreCollision(plate, ignore[i], true);
        }
    }

    void FaceCamera()
    {
        Camera cam = GameController.Instance != null ? GameController.Instance.playerCamera : null;
        if (cam == null)
            return;

        transform.rotation = cam.transform.rotation;
    }
}
