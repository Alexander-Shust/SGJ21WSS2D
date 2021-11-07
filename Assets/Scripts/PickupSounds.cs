using UnityEngine;

public class PickupSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip firstSound;
    [SerializeField]
    private AudioClip secondSound;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void PlaySoundOne()
    {
        _audioSource.PlayOneShot(firstSound);
    }

    private void PlaySoundTwo()
    {
        _audioSource.PlayOneShot(secondSound);
    }
    
    public void PlayRandomSound()
    {
        var number = Random.Range(0, 2);
        switch (number)
        {
            case 0:
                PlaySoundOne();
                break;
            case 1:
                PlaySoundTwo();
                break;
        }
    }
}