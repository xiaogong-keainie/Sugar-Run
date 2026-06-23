using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    public string videoFileName = "大肠内部.mp4";
    public Color fallbackColor = new Color(0.5f, 0.7f, 1f);
    VideoPlayer vp;
    RenderTexture rt;
    GameObject quad;
    Material mat;

    void Awake()
    {
        var cam = GetComponent<Camera>();
        vp = GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.RenderTexture;

        if (vp.clip == null && string.IsNullOrEmpty(vp.url))
        {
            vp.source = VideoSource.Url;
            vp.url = Application.dataPath + "/" + videoFileName;
        }

        rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        vp.targetTexture = rt;

        // Quad with fallback color (video will replace it when ready)
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "VideoBackgroundQuad";
        Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = new Vector3(0, 0, cam.farClipPlane * 0.99f);

        // 1x1 texture with fallback color for when video is off
        var fallbackTex = new Texture2D(1, 1);
        fallbackTex.SetPixel(0, 0, fallbackColor);
        fallbackTex.Apply();

        mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = fallbackTex;
        quad.GetComponent<Renderer>().material = mat;

        SizeQuadToView();
        StartCoroutine(SizeQuadDelayed());

        vp.prepareCompleted += OnPrepared;
        vp.Prepare();
    }

    IEnumerator SizeQuadDelayed()
    {
        // Retry sizing after CameraFollow adjusts ortho size
        yield return new WaitForEndOfFrame();
        SizeQuadToView();
    }

    void SizeQuadToView()
    {
        var cam = GetComponent<Camera>();
        if (cam == null || quad == null) return;

        float h = cam.orthographicSize * 2f;
        float w = h * (16f / 9f);
        quad.transform.localScale = new Vector3(w, h, 1);
    }

    void LateUpdate()
    {
        SizeQuadToView();
    }

    void OnPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnPrepared;
        mat.mainTexture = rt;
        source.Play();
    }

    void OnDestroy()
    {
        if (rt != null)
            rt.Release();
    }
}
