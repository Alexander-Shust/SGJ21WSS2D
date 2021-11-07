using Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public static class DialogExtension
{
    public static void ShowDialog(this SystemBase system, string message, float duration = 1.0f)
    {
        var dialogPrefab = system.GetPrefab<DialogComponent>();
        if (dialogPrefab == Entity.Null)
            return;
        var dialogEntity = system.EntityManager.Instantiate(dialogPrefab);
        system.EntityManager.SetComponentData(dialogEntity, new DialogComponent {Value = duration});
        var text = system.EntityManager.GetComponentObject<Text>(dialogEntity);
        var canvas = GameObject.FindWithTag("MyCanvas").transform;
        text.text = message;
        text.transform.SetParent(canvas, false);
        var bubble = GameObject.FindWithTag("Bubble");
        bubble.GetComponent<RawImage>().enabled = true;
        var transform = bubble.GetComponent<RectTransform>();
        var scale = GetSizeFromString(message);
        transform.localScale = new Vector3(scale, scale, 1.0f);
    }

    private static float GetSizeFromString(string message)
    {
        var length = message.Length;
        var result = 7.0f;
        if (length > 10)
            result += 2.0f;
        if (length > 30)
            result += 3.0f;
        if (length > 50)
            result += 3.0f;
        return result;
    }
}