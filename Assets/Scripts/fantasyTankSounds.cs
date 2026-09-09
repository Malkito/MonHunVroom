using UnityEngine;
using Ami.Extension;
using Ami.BroAudio;


public class fantasyTankSounds : MonoBehaviour
{
    [SerializeField] private SoundID FantasyFireSFX;
    [SerializeField] private SoundID TronFireSFX;
    [SerializeField] private SoundID FantasyMovementSFX;
    [SerializeField] private SoundID TronMovementSFX;
    [SerializeField] private SoundID JumpSoundSFX;
    [SerializeField] private SoundID alteranteFireSFX;



    public void playMovementSound(bool IsFantasy, float moveSpeed)
    {

        if (IsFantasy)
        {
            BroAudio.Play(FantasyMovementSFX);
        }
        else
        {
            BroAudio.Play(TronMovementSFX);
        }

    }


    public void PlayJumpSound()
    {
        BroAudio.Play(JumpSoundSFX);
    }



    public void playFireSound(bool IsFantasy)
    {
        if (IsFantasy)
        {
            BroAudio.Play(FantasyFireSFX);
        }
        else
        {
            BroAudio.Play(TronFireSFX);
        }
    }

    public void playerAlternateFire()
    {
        BroAudio.Play(alteranteFireSFX);
    }





}
