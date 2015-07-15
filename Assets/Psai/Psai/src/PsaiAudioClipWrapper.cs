#if (!PSAI_STANDALONE)

/// <summary>
/// This wrapper is needed to avoid a bug in Unity 4.x that prevents AudioClips from streaming from disk.
/// </summary>
public class PsaiAudioClipWrapper : UnityEngine.MonoBehaviour
{
    public UnityEngine.AudioClip _audioClip;
}

#endif