using System;
using UnityEngine;
using UnityEngine.UI;

public class BubbleTime : MonoBehaviour
{
    public float time;

    private void Start()
    {
        
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var text = GetComponent<RawImage>();
        if (text.enabled)
            time -= deltaTime;
        if (time <= 0)
        {
            text.enabled = false;
            time = 1.0f;
        }
    }
}