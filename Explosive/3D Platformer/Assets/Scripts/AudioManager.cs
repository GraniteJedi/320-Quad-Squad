using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource UISource;

    [Header("UI SFX")]
    [SerializeField] private AudioClip countdownTick;
    [SerializeField] private AudioClip countdownClick;
    //[SerializeField] private AudioClip menuOpen;

    //[Header("Input SFX")]

    //[Header("Soundtrack")]

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayCountdownTick()
    {
        UISource.PlayOneShot(countdownTick);
    }

    public void PlayCountdownClicks()
    {
        UISource.PlayOneShot(countdownClick);
    }

    public void TimeShift(float time)
    {
        UISource.pitch = time;
    }
}
