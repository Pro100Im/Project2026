using System.Collections.Generic;
using Code.Meta.Features.Game;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Game.Features.Exchequer
{
    public class GameExchequerView : MonoBehaviour
    {
        private const float PopDuration = 0.15f;
        private static readonly Scale NormalScale = new(Vector2.one);
        private static readonly Scale PoppedScale = new(new Vector2(1.15f, 1.15f));

        private GameScreen _gameScreen;

        private Label _mealScore;
        private Label _manaScore;
        private Label _goldScore;

        private int _mealValue;
        private int _manaValue;
        private int _goldValue;

        private IVisualElementScheduledItem _mealPopReset;
        private IVisualElementScheduledItem _manaPopReset;
        private IVisualElementScheduledItem _goldPopReset;

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

            SetupPopTransition(_mealScore);
            SetupPopTransition(_manaScore);
            SetupPopTransition(_goldScore);

            _mealScore.text = 0.ToString();
            _manaScore.text = 0.ToString();
            _goldScore.text = 0.ToString();
        }

        public void SetMeal(int value)
        {
            if (value != _mealValue)
                _mealPopReset = PlayPopAnimation(_mealScore, _mealPopReset);

            _mealValue = value;
            _mealScore.text = value.ToString();
        }

        public void SetMana(int value)
        {
            if (value != _manaValue)
                _manaPopReset = PlayPopAnimation(_manaScore, _manaPopReset);

            _manaValue = value;
            _manaScore.text = value.ToString();
        }

        public void SetGold(int value)
        {
            if (value != _goldValue)
                _goldPopReset = PlayPopAnimation(_goldScore, _goldPopReset);

            _goldValue = value;
            _goldScore.text = value.ToString();
        }

        private static void SetupPopTransition(VisualElement element)
        {
            element.style.transitionProperty = new List<StylePropertyName> { "scale" };
            element.style.transitionDuration = new List<TimeValue> { new(PopDuration, TimeUnit.Second) };
            element.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.EaseOutSine };
        }

        private static IVisualElementScheduledItem PlayPopAnimation(VisualElement element, IVisualElementScheduledItem pendingReset)
        {
            pendingReset?.Pause();

            element.style.scale = PoppedScale;

            return element.schedule
                .Execute(() => element.style.scale = NormalScale)
                .StartingIn((long)(PopDuration * 1000));
        }

        private void OnDestroy()
        {
            _mealPopReset?.Pause();
            _manaPopReset?.Pause();
            _goldPopReset?.Pause();
        }
    }
}
