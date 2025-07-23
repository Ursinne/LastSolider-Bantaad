using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class BirdRandomizer : MonoBehaviour
{
    public AudioClip[] birdSounds;
    public AudioMixer birdMixer;
    public float xMaxOffset, xMinOffset, yMaxOffset, yMinOffset, zMaxOffset, zMinOffset;
    float timeOffset;
    public float minSoundDelay, maxSoundDelay;
    void Start()
    {
        timeOffset = Random.Range(minSoundDelay, maxSoundDelay);
        Invoke("PlayBirdSound", timeOffset);
    }
    void PlayBirdSound()
    {
        GameObject birdObject = new GameObject("BirdObject");
        AudioSource birdSource = birdObject.AddComponent<AudioSource>();
        birdSource.clip = birdSounds[Random.Range(0, birdSounds.Length)];
        birdSource.spatialBlend = 1f;

        float xPos = Random.Range(xMinOffset, xMaxOffset);
        float yPos = Random.Range(yMinOffset, yMaxOffset);
        float zPos = Random.Range(zMinOffset, zMaxOffset);
        birdObject.transform.position = new Vector3(xPos, yPos, zPos) + transform.position;
        birdSource.outputAudioMixerGroup = birdMixer.FindMatchingGroups("Birds")[0];
        birdSource.Play();
        Destroy(birdObject, birdSource.clip.length);
        timeOffset = Random.Range(minSoundDelay, maxSoundDelay);
        Invoke("PlayBirdSound", timeOffset);
    }
}