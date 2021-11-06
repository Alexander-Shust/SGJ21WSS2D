using Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    public class GameOverSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameOverComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<GameOverComponent>());
            var aliveTraps = 0;
            Entities.WithAll<GirlComponent>()
                .ForEach((Entity trapEntity) =>
                {
                    ++aliveTraps;
                }).WithoutBurst().Run();
            if (aliveTraps > 0)
            {
                var loseText = GameObject.FindWithTag("LoseText");
                loseText.GetComponent<Text>().enabled = true;
            }
            else
            {
                var winText = GameObject.FindWithTag("WinText");
                winText.GetComponent<Text>().enabled = true;
            }
            var gameOverText = GameObject.FindWithTag("GameOver");
            gameOverText.GetComponent<Text>().enabled = true;
            var buttonQuit = GameObject.FindWithTag("QuitButton");
            buttonQuit.GetComponent<Image>().enabled = true;
            buttonQuit.GetComponentInChildren<Text>().enabled = true;
            buttonQuit.GetComponent<Button>().onClick.AddListener(QuitClick);
        }
        
        private void QuitClick()
        {
            Application.Quit();
        }
    }
}