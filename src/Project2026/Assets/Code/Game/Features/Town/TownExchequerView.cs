using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Game.Features.Town
{
    public class TownExchequerView : MonoBehaviour
    {
        private const float PopDuration = 0.15f;
        private static readonly Scale NormalScale = new(Vector2.one);
        private static readonly Scale PoppedScale = new(new Vector2(1.15f, 1.15f));

        private TownScreen _townScreen;

        private Label _woodScore;
        private Label _stoneScore;
        private Label _crystalScore;
        private Label _knowledgeScore;
        private Label _goldScore;

        private int _woodValue;
        private int _stoneValue;
        private int _crystalValue;
        private int _knowledgeValue;
        private int _goldValue;

        private IVisualElementScheduledItem _woodPopReset;
        private IVisualElementScheduledItem _stonePopReset;
        private IVisualElementScheduledItem _crystalPopReset;
        private IVisualElementScheduledItem _knowledgePopReset;
        private IVisualElementScheduledItem _goldPopReset;

        [Inject]
        public void Construct(TownScreen townScreen)
        {
            _townScreen = townScreen;
        }

        private void Start()
        {
            _woodScore = _townScreen.GetLabel("WoodScoreValue");
            _stoneScore = _townScreen.GetLabel("StoneScoreValue");
            _crystalScore = _townScreen.GetLabel("DymondScoreValue");
            _knowledgeScore = _townScreen.GetLabel("KnowledgeScoreValue");
            _goldScore = _townScreen.GetLabel("GoldScoreValue");

            SetupPopTransition(_woodScore);
            SetupPopTransition(_stoneScore);
            SetupPopTransition(_crystalScore);
            SetupPopTransition(_knowledgeScore);
            SetupPopTransition(_goldScore);

            _woodScore.text = 0.ToString();
            _stoneScore.text = 0.ToString();
            _crystalScore.text = 0.ToString();
            _knowledgeScore.text = 0.ToString();
            _goldScore.text = 0.ToString();
        }

        public void SetWood(int value)
        {
            if (value != _woodValue)
                _woodPopReset = PlayPopAnimation(_woodScore, _woodPopReset);

            _woodValue = value;
            _woodScore.text = value.ToString();
        }

        public void SetStone(int value)
        {
            if (value != _stoneValue)
                _stonePopReset = PlayPopAnimation(_stoneScore, _stonePopReset);

            _stoneValue = value;
            _stoneScore.text = value.ToString();
        }

        public void SetCrystal(int value)
        {
            if (value != _crystalValue)
                _crystalPopReset = PlayPopAnimation(_crystalScore, _crystalPopReset);

            _crystalValue = value;
            _crystalScore.text = value.ToString();
        }

        public void SetKnowledge(int value)
        {
            if (value != _knowledgeValue)
                _knowledgePopReset = PlayPopAnimation(_knowledgeScore, _knowledgePopReset);

            _knowledgeValue = value;
            _knowledgeScore.text = value.ToString();
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
            _woodPopReset?.Pause();
            _stonePopReset?.Pause();
            _crystalPopReset?.Pause();
            _knowledgePopReset?.Pause();
            _goldPopReset?.Pause();
        }
    }
}
