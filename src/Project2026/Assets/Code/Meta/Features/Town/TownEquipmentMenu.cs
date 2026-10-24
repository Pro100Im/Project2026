using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Town
{
    public class TownEquipmentMenu : MonoBehaviour
    {
        private const string SelectedClass = "equipment-slot--selected";

        [SerializeField] private TownScreen _townScreen;

        private UIService _uIService;

        private VisualElement _menu;
        private VisualElement _choices;
        private Label _empty;
        private Button _closeButton;
        private Button _clearButton;
        private readonly Slot[] _slots = new Slot[UnitRoster.SlotCount];
        private readonly List<UnitKind> _choiceKinds = new(4);

        private int _selectedSlot = -1;
        private int _visibilityVersion;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            _menu = _townScreen.GetVisualElement("TownEquipmentMenu");
            _choices = _townScreen.GetVisualElement("TownEquipmentChoices");
            _empty = _townScreen.GetLabel("TownEquipmentEmpty");
            _closeButton = _townScreen.GetButton("TownEquipmentCloseButton");
            _clearButton = _townScreen.GetButton("TownEquipmentClearButton");

            for (var i = 0; i < _slots.Length; i++)
            {
                var button = _townScreen.GetButton($"EquipmentSlot{i}");
                var slot = i;
                button.clickable.clicked += () => Select(slot);
                _slots[i] = new Slot(button, button.Q<Image>("Icon"), button.Q<Label>("Title"));
            }

            _closeButton.clickable.clicked += CloseRequest;
            _clearButton.clickable.clicked += ClearRequest;
            ShowChoices(false);

            _menu.style.display = DisplayStyle.None;
            _uIService.Hide(_menu).Forget();
        }

        public void Show()
        {
            _visibilityVersion++;
            _menu.style.display = DisplayStyle.Flex;
            _uIService.Show(_menu).Forget();
        }

        public void Hide()
        {
            _selectedSlot = -1;
            RefreshSelection();
            ShowChoices(false);
            HideAsync(++_visibilityVersion).Forget();
        }

        public void SetSlot(int index, Sprite icon, string title)
        {
            if (index < 0 || index >= _slots.Length || _slots[index] == null)
                return;

            var slot = _slots[index];
            slot.Icon.sprite = icon;
            slot.Title.text = title;
        }

        public void SetChoices(List<EquipmentChoice> choices)
        {
            if (!SameChoices(choices))
            {
                _choices.Clear();
                _choiceKinds.Clear();

                for (var i = 0; i < choices.Count; i++)
                {
                    var choice = choices[i];
                    _choiceKinds.Add(choice.Kind);
                    _choices.Add(CreateChoice(choice));
                }
            }

            RefreshEmpty();
        }

        private void Select(int index)
        {
            _selectedSlot = _selectedSlot == index ? -1 : index;
            RefreshSelection();
            ShowChoices(_selectedSlot >= 0);
        }

        private void RefreshSelection()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null)
                    continue;

                _slots[i].Button.EnableInClassList(SelectedClass, i == _selectedSlot);
            }
        }

        private void ShowChoices(bool visible)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            _choices.style.display = display;
            _clearButton.style.display = display;
            RefreshEmpty();
        }

        private void RefreshEmpty()
        {
            var show = _selectedSlot >= 0 && _choiceKinds.Count == 0;
            _empty.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private Button CreateChoice(EquipmentChoice choice)
        {
            var button = new Button();
            button.AddToClassList("build-slot");

            var icon = new Image();
            icon.AddToClassList("build-slot__icon");
            icon.pickingMode = PickingMode.Ignore;
            icon.sprite = choice.Icon;

            var title = new Label(choice.Title);
            title.AddToClassList("commonText");
            title.AddToClassList("build-slot__title");
            title.pickingMode = PickingMode.Ignore;

            button.Add(icon);
            button.Add(title);

            var kind = choice.Kind;
            button.clickable.clicked += () => Assign(kind);
            return button;
        }

        private bool SameChoices(List<EquipmentChoice> choices)
        {
            if (choices.Count != _choiceKinds.Count)
                return false;

            for (var i = 0; i < choices.Count; i++)
            {
                if (choices[i].Kind != _choiceKinds[i])
                    return false;
            }

            return true;
        }

        private void Assign(UnitKind kind)
        {
            if (_selectedSlot < 0)
                return;

            var request = CreateInputEntity.Empty();
            request.AddTownEquipmentAssignRequested(_selectedSlot, kind);
            request.isInput = true;
        }

        private void ClearRequest()
        {
            if (_selectedSlot < 0)
                return;

            var request = CreateInputEntity.Empty();
            request.AddTownEquipmentClearRequested(_selectedSlot);
            request.isInput = true;
        }

        private void CloseRequest()
        {
            var request = CreateInputEntity.Empty();
            request.isTownEquipmentMenuCloseRequested = true;
            request.isInput = true;
        }

        private async UniTaskVoid HideAsync(int version)
        {
            await _uIService.Hide(_menu);

            if (version == _visibilityVersion)
                _menu.style.display = DisplayStyle.None;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.clickable.clicked -= CloseRequest;
            if (_clearButton != null)
                _clearButton.clickable.clicked -= ClearRequest;
        }

        public readonly struct EquipmentChoice
        {
            public EquipmentChoice(UnitKind kind, Sprite icon, string title)
            {
                Kind = kind;
                Icon = icon;
                Title = title;
            }

            public UnitKind Kind { get; }
            public Sprite Icon { get; }
            public string Title { get; }
        }

        private sealed class Slot
        {
            public readonly Button Button;
            public readonly Image Icon;
            public readonly Label Title;

            public Slot(Button button, Image icon, Label title)
            {
                Button = button;
                Icon = icon;
                Title = title;
            }
        }
    }
}
