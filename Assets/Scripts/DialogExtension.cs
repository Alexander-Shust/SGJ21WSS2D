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
    }
}