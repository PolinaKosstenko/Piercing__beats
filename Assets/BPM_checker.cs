using UnityEngine;

public class UniBpmAnalyzerExample : MonoBehaviour
{
    [SerializeField]
    private AudioClip targetClip;

    private void Start()
    {
        int bpm = UniBpmAnalyzer.AnalyzeBpm(targetClip);
        if (bpm < 0)
        {
            Debug.LogError("AudioClip is null.");
            return;
        }
    }
}