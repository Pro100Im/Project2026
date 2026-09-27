using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.Features.Town;
using Code.Game.StaticData.Configs;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Town
{
    public class TownBuildMenu : MonoBehaviour
    {
        private const string BuiltClass = "build-slot--built";
        private const string AvailableClass = "build-slot--available";
        private const string LockedClass = "build-slot--locked";

        [SerializeField] private TownScreen _townScreen;
        [SerializeField] private VisualTreeAsset _slotTemplate;

        private UIService _uIService;

        private VisualElement _menu;
        private VisualElement _columnsContainer;
        private Button _closeButton;

        private readonly Dictionary<BuildingType, VisualElement> _columns = new(16);
        private readonly Dictionary<BuildingType, List<Slot>> _slots = new(16);

        private int _visibilityVersion;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            _menu = _townScreen.GetVisualElement("TownBuildMenu");
            _columnsContainer = _townScreen.GetVisualElement("TownBuildColumns");
            _closeButton = _townScreen.GetButton("TownBuildCloseButton");

            _closeButton.clickable.clicked += CloseRequest;

            CreateColumns();

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
            HideAsync(++_visibilityVersion).Forget();
        }

        public void SetSlot(BuildingType type, int levelIndex, TownBuildingLevelData data, TownBuildSlotState state)
        {
            var slot = GetOrCreateSlot(type, levelIndex);

            slot.Icon.sprite = data.Icon;
            slot.Title.text = data.Title;
            slot.State.text = GetStateText(state);

            slot.Button.EnableInClassList(BuiltClass, state == TownBuildSlotState.Built);
            slot.Button.EnableInClassList(AvailableClass, state == TownBuildSlotState.Available);
            slot.Button.EnableInClassList(LockedClass, state == TownBuildSlotState.Locked);
            slot.Button.SetEnabled(state == TownBuildSlotState.Available);
        }

        private async UniTaskVoid HideAsync(int version)
        {
            await _uIService.Hide(_menu);

            if (version == _visibilityVersion)
                _menu.style.display = DisplayStyle.None;
        }

        private void CreateColumns()
        {
            var types = (BuildingType[])Enum.GetValues(typeof(BuildingType));

            for (var i = 0; i < types.Length; i++)
            {
                var column = new VisualElement();
                column.AddToClassList("build-column");
                column.pickingMode = PickingMode.Ignore;

                _columnsContainer.Add(column);
                _columns[types[i]] = column;
                _slots[types[i]] = new List<Slot>(4);
            }
        }

        private Slot GetOrCreateSlot(BuildingType type, int levelIndex)
        {
            var slots = _slots[type];

            while (slots.Count <= levelIndex)
            {
                var root = _slotTemplate.Instantiate();
                root.pickingMode = PickingMode.Ignore;

                var slot = new Slot(
                    root.Q<Button>("Slot"),
                    root.Q<Image>("Icon"),
                    root.Q<Label>("Title"),
                    root.Q<Label>("State"));

                slot.Button.clickable.clicked += () => BuildRequest(type);

                _columns[type].Add(root);
                slots.Add(slot);
            }

            return slots[levelIndex];
        }

        private static string GetStateText(TownBuildSlotState state) =>
            state switch
            {
                TownBuildSlotState.Built => "Built",
                TownBuildSlotState.Available => "Build",
                _ => "Locked"
            };

        private void BuildRequest(BuildingType type)
        {
            var request = CreateInputEntity.Empty();
            request.AddTownBuildRequested(type);
            request.isInput = true;
        }

        private void CloseRequest()
        {
            var request = CreateInputEntity.Empty();
            request.isTownBuildMenuCloseRequested = true;
            request.isInput = true;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.clickable.clicked -= CloseRequest;
        }

        private readonly struct Slot
        {
            public readonly Button Button;
            public readonly Image Icon;
            public readonly Label Title;
            public readonly Label State;

            public Slot(Button button, Image icon, Label title, Label state)
            {
                Button = button;
                Icon = icon;
                Title = title;
                State = state;
            }
        }
    }
}
