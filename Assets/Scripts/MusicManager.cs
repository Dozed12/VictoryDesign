using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    //Main Menu Object
    public GameObject mainMenu;

    //Audio Player
    public AudioSource player;

    //Main Menu Music
    public AudioClip menuMusic;

    //Musics
    public List<AudioClip> musicList;

    //Previous Song
    public AudioClip previous;

    //Start
    void Start()
    {
        mainMenu = GameObject.Find("MainMenu");

        previous = musicList[0];
    }

    //Update
    void Update()
    {
        //If in Main Menu
        if (mainMenu.activeInHierarchy)
        {
            //If not playing start in loop
            if (!player.isPlaying)
            {
                //If SFX is playing wait
                if (GameObject.Find("SFXManager").GetComponent<AudioSource>().isPlaying)
                    return;

                player.clip = menuMusic;
                player.loop = true;
                player.Play();
            }
        }
        //If not in Main Menu
        else
        {
            //Turn off loop
            player.loop = false;

            //Play random music
            if (!player.isPlaying)
            {
                //Pick song that isnt same as previous
                do
                {
                    player.clip = musicList[Random.Range(0, musicList.Count)];
                } while (previous == player.clip);

                //Previous is new song
                previous = player.clip;

                //Play with 1 sec delay
                player.PlayDelayed(1);
            }
        }
    }
}
