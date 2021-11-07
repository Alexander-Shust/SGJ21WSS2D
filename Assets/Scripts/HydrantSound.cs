using UnityEngine;

public class HydrantSound : MonoBehaviour
{
    [SerializeField] private AudioClip sound;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        _audioSource.PlayOneShot(sound);
    }
}