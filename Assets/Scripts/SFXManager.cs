using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    //Audio Player
    public AudioSource player;

    //Signature
    public AudioClip signature;
    public void PlaySignature()
    {
        if (!player.isPlaying)
        {
            player.clip = signature;
            player.Play();
        }
    }

    //Stamp
    public AudioClip stamp;
    public void PlayStamp()
    {
        if (!player.isPlaying)
        {
            player.clip = stamp;
            player.Play();
        }
    }

    //Double Explosion
    public AudioClip explosion;
    public void PlayDoubleExplosion()
    {
        if (!player.isPlaying)
        {
            player.clip = explosion;
            player.Play();
        }
    }
}
