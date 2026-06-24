using UnityEngine;
using UnityEngine.Video;

public class VideoBackground : MonoBehaviour
{
    public VideoClip videoClip;

    private VideoPlayer vp;

    void Awake()
    {
        vp = gameObject.AddComponent<VideoPlayer>();
        vp.playOnAwake = true;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.CameraFarPlane;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;
        vp.controlledAudioTrackCount = 1;

        if (videoClip != null)
        {
            vp.clip = videoClip;
            vp.source = VideoSource.VideoClip;
        }

        vp.SetDirectAudioVolume(0, GameManager.Instance != null ? GameManager.Instance.GetVolume() : 1f);

        Force16To9();
    }

    public void SetVolume(float volume)
    {
        if (vp != null && vp.controlledAudioTrackCount > 0)
            vp.SetDirectAudioVolume(0, volume);
    }

    void Force16To9()
    {
        var cam = GetComponent<Camera>();

        float target = 16f / 9f;
        float current = (float)Screen.width / Screen.height;
        float scale = current / target;

        if (scale < 1f)
        {
            var rect = cam.rect;
            rect.width = 1f;
            rect.height = scale;
            rect.x = 0;
            rect.y = (1f - scale) / 2f;
            cam.rect = rect;
        }
        else
        {
            var rect = cam.rect;
            rect.width = 1f / scale;
            rect.height = 1f;
            rect.x = (1f - 1f / scale) / 2f;
            rect.y = 0;
            cam.rect = rect;
        }

        if (!Application.isEditor)
            Screen.fullScreen = true;
    }
}
