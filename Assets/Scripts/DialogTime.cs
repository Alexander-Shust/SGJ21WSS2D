using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogTime : MonoBehaviour
{
    public float time;

    private void Start()
    {
        
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var text = GetComponent<Text>();
        if (text.enabled)
            time -= deltaTime;
        if (time <= 0)
        {
            text.enabled = false;
            time = 1.0f;
        }
    }
}