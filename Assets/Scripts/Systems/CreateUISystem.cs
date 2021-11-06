using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    public class CreateUISystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<CreateUI>();
            EntityManager.CreateEntity(typeof(CreateUI));
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<CreateUI>());
            var gameOverText = GameObject.FindWithTag("GameOver");
            gameOverText.GetComponent<Text>().enabled = false;
            var winText = GameObject.FindWithTag("WinText");
            winText.GetComponent<Text>().enabled = false;
            var loseText = GameObject.FindWithTag("LoseText");
            loseText.GetComponent<Text>().enabled = false;
            var buttonQuit = GameObject.FindWithTag("QuitButton");
            buttonQuit.GetComponent<Image>().enabled = false;
            buttonQuit.GetComponentInChildren<Text>().enabled = false;
        }

        public struct CreateUI : IComponentData
        {
        
        }
    }
}