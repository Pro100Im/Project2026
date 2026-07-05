using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class GameExchequerScore : MonoBehaviour
    {
        private GameScreen _gameScreen;

        private Label _mealScore;
        private Label _manaScore;
        private Label _goldScore;

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _mealScore = _gameScreen.GetLabel("MealScoreValue");
            _manaScore = _gameScreen.GetLabel("ManaScoreValue");
            _goldScore = _gameScreen.GetLabel("GoldScoreValue");

            _mealScore.text = 0.ToString();
            _manaScore.text = 0.ToString();
            _goldScore.text = 0.ToString();
        }
    }
}