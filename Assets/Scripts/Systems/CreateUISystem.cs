using Components;
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
            RequireSingletonForUpdate<GameSettingsComponent>();
            EntityManager.CreateEntity(typeof(CreateUI));
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<CreateUI>());

            var settings = GetSingleton<GameSettingsComponent>();
            var gameOverText = GameObject.FindWithTag("GameOver");
            gameOverText.GetComponent<Text>().enabled = false;
            var winText = GameObject.FindWithTag("WinText");
            winText.GetComponent<Text>().enabled = false;
            var loseText = GameObject.FindWithTag("LoseText");
            loseText.GetComponent<Text>().enabled = false;
            var buttonQuit = GameObject.FindWithTag("QuitButton");
            buttonQuit.GetComponent<Image>().enabled = false;
            buttonQuit.GetComponentInChildren<Text>().enabled = false;
            var highScoreText = GameObject.FindWithTag("HighScore");
            var highScore = PlayerPrefs.GetFloat("highscore");
            highScoreText.GetComponent<Text>().text = ((int) highScore).ToString();
            var scoreEntity = EntityManager.CreateEntity(typeof(ScoreComponent));
            EntityManager.SetComponentData(scoreEntity, new ScoreComponent {Value = settings.StartingScore});
        }

        public struct CreateUI : IComponentData
        {
        
        }
    }
}