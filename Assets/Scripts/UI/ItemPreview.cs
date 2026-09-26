using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ItemPreview : MonoBehaviour
{
    const int PreviewLayer = 3;
    const float SwayDegrees = 30f;
    const float SwaySpeed = 1.2f;

    static int nextPlace;

    [SerializeField] Transform modelAnchor;
    [SerializeField] Camera previewCamera;

    RenderTexture texture;
    GameObject model;
    float swayStart;

    public RenderTexture Texture => texture;

    void Awake()
    {
        int place = nextPlace++;
        transform.position = new Vector3(place * 8f, -40f, 0f);

        texture = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
        texture.Create();
        previewCamera.targetTexture = texture;
        previewCamera.cullingMask = 1 << PreviewLayer;
        UniversalAdditionalCameraData cameraData = previewCamera.GetUniversalAdditionalCameraData();
        cameraData.renderShadows = false;
    }

    public void SetModel(GameObject itemModel, Color color)
    {
        ClearModel();
        if (itemModel == null || modelAnchor == null)
            return;

        swayStart = Time.time;
        model = Instantiate(itemModel, modelAnchor);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        SetLayer(model);

        Renderer renderer = model.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material.color = color;
        }

        previewCamera.enabled = true;
    }

    public void ClearModel()
    {
        if (model != null)
            Destroy(model);
        model = null;
        if (previewCamera != null)
            previewCamera.enabled = false;
    }

    void Update()
    {
        if (model == null)
            return;

        float angle = Mathf.Sin((Time.time - swayStart) * SwaySpeed) * SwayDegrees;
        model.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    void OnDestroy()
    {
        if (previewCamera != null)
            previewCamera.targetTexture = null;
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }
    }

    static void SetLayer(GameObject view)
    {
        view.layer = PreviewLayer;
        foreach (Transform child in view.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = PreviewLayer;
    }
}
