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
        player.clip = signature;
        player.Play();
    }

    //Stamp
    public AudioClip stamp;
    public void PlayStamp()
    {
        player.clip = stamp;
        player.Play();
    }

    //Double Explosion
    public AudioClip explosion;
    public void PlayDoubleExplosion()
    {
        player.clip = explosion;
        player.Play();
    }

    //Click
    public AudioClip click;
    public void PlayClick()
    {
        player.clip = click;
        player.Play();
    }
}
