using UnityEngine;
using Ami.BroAudio;

public class backgroundMusicPlayer : MonoBehaviour
{
    [SerializeField] private SoundID BGM;

    private void Start()
    {
        BroAudio.Play(BGM);
    }



}
