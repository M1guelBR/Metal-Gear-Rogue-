using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Musica : MonoBehaviour
{
    public List<AudioClip> musica = new List<AudioClip>();
    List<AudioClip> usados = new List<AudioClip>();
    public AudioClip alertTheme;
    AudioSource source;
    float defVol;
    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        defVol = source.volume;
        source.volume = 0;
        RepMusica();

    }

    // Update is called once per frame
    void Update()
    {
        if(source.volume < defVol)
        {
            source.volume += Time.deltaTime;
            if (source.volume > defVol)
                source.volume = defVol;
        }
        if (!source.isPlaying)
            RepMusica();
    }
    public void AlertaMusica()
    {
        //Reproduce el tema
        AudioSource source = GetComponent<AudioSource>();
        source.Stop();
        source.clip = alertTheme;
        source.loop = false;
        source.Play();

    }
    public void ResetMusicaCola()
    {
        musica.AddRange(usados);
        usados = new List<AudioClip>();
        this.GetComponent<AudioSource>().loop = false;
    }
    public void RepMusica()
    {
        int i = Random.Range(0, musica.Count);

        //Reproduce el tema
        AudioSource source = GetComponent<AudioSource>();
        source.Stop();
        source.clip = musica[i];
        source.loop = false;
        source.Play();

        usados.Add(musica[i]);
        musica.RemoveAt(i);

    }

}
