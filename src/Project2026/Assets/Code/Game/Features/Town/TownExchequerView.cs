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

            WriteScore(_woodScore, _woodValue);
            WriteScore(_stoneScore, _stoneValue);
            WriteScore(_crystalScore, _crystalValue);
            WriteScore(_knowledgeScore, _knowledgeValue);
            WriteScore(_goldScore, _goldValue);
        }

        public void SetWood(int value, bool animate = true) => SetScore(_woodScore, ref _woodValue, ref _woodPopReset, value, animate);

        public void SetStone(int value, bool animate = true) => SetScore(_stoneScore, ref _stoneValue, ref _stonePopReset, value, animate);

        public void SetCrystal(int value, bool animate = true) => SetScore(_crystalScore, ref _crystalValue, ref _crystalPopReset, value, animate);

        public void SetKnowledge(int value, bool animate = true) => SetScore(_knowledgeScore, ref _knowledgeValue, ref _knowledgePopReset, value, animate);

        public void SetGold(int value, bool animate = true) => SetScore(_goldScore, ref _goldValue, ref _goldPopReset, value, animate);

        private static void SetScore(Label label, ref int current, ref IVisualElementScheduledItem popReset, int value, bool animate)
        {
            var changed = value != current;
            current = value;

            if (label == null)
                return;

            if (animate && changed)
                popReset = PlayPopAnimation(label, popReset);

            label.text = value.ToString();
        }

        private static void WriteScore(Label label, int value)
        {
            if (label != null)
                label.text = value.ToString();
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
