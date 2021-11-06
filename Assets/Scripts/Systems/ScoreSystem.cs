using Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Systems
{
    public class ScoreSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ScoreComponent>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var scoreEntity = GetSingletonEntity<ScoreComponent>();
            var currentScore = EntityManager.GetComponentData<ScoreComponent>(scoreEntity).Value;
            currentScore -= deltaTime;
            var scoreText = GameObject.FindWithTag("Score");
            scoreText.GetComponent<Text>().text = ((int) currentScore).ToString();
            EntityManager.SetComponentData(scoreEntity, new ScoreComponent {Value = currentScore});
        }
    }
}