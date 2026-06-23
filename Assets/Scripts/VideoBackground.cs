using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    VideoPlayer vp;
    RenderTexture rt;
    GameObject quad;

    void Awake()
    {
        var cam = GetComponent<Camera>();
        vp = GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.RenderTexture;

        if (vp.clip == null)
        {
            vp.source = VideoSource.Url;
            vp.url = Application.dataPath + "/大肠内部.mp4";
        }

        rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        vp.targetTexture = rt;

        // Quad behind everything
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "VideoBackgroundQuad";
        Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = new Vector3(0, 0, cam.farClipPlane * 0.99f);

        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
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
        source.Play();
    }

    void OnDestroy()
    {
        if (rt != null)
            rt.Release();
    }
}
