using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    VideoPlayer vp;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = true;
        vp.prepareCompleted += OnPrepared;
        vp.Prepare();
    }

    void OnPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnPrepared;
        source.Play();
    }
}
