using Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public static class DialogExtension
{
    public static void ShowDialog(this SystemBase system, string message, float duration = 1.0f)
    {
        var offset = GetOffsetFromString(message);
        var canvas = GameObject.FindWithTag("MyCanvas").transform;
        var textObject = GameObject.FindWithTag("Dialog");
        var text = textObject.GetComponent<Text>();
        var dialogComponent = textObject.GetComponent<DialogTime>();
        dialogComponent.time = duration;
        text.enabled = true;
        text.text = message;
        var textPosition = text.transform.position;
        text.transform.position = new Vector3(textPosition.x, offset, textPosition.z);
        text.transform.SetParent(canvas, false);
        var bubble = GameObject.FindWithTag("Bubble");
        var bubbleComponent = bubble.GetComponent<BubbleTime>();
        bubbleComponent.time = duration;
        bubble.GetComponent<RawImage>().enabled = true;
        var transform = bubble.GetComponent<RectTransform>();
        var scale = GetScaleFromString(message);
        transform.localScale = new Vector3(scale, scale, 1.0f);
        var bubblePosition = transform.position;
        transform.position = new Vector3(bubblePosition.x, offset, bubblePosition.z);
    }

    private static float GetOffsetFromString(string message)
    {
        var length = message.Length;
        var result = 550.0f;
        if (length > 10)
            result += 20.0f;
        if (length > 30)
            result += 20.0f;
        if (length > 50)
            result += 30.0f;
        return result;
    }

    private static float GetScaleFromString(string message)
    {
        var length = message.Length;
        var result = 7.0f;
        if (length > 10)
            result += 2.0f;
        if (length > 30)
            result += 2.0f;
        if (length > 50)
            result += 3.0f;
        return result;
    }
}