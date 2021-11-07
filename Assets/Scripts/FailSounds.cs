using UnityEngine;

public class FailSounds : MonoBehaviour
{
    [SerializeField]
    private AudioClip firstSound;
    [SerializeField]
    private AudioClip secondSound;
    [SerializeField]
    private AudioClip thirdSound;

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
    
    private void PlaySoundThree()
    {
        _audioSource.PlayOneShot(thirdSound);
    }

    public void PlayRandomSound()
    {
        var number = Random.Range(0, 3);
        switch (number)
        {
            case 0:
                PlaySoundOne();
                break;
            case 1:
                PlaySoundTwo();
                break;
            case 2:
                PlaySoundThree();
                break;
        }
    }
}