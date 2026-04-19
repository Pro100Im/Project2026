using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common.UI
{
    public class UIService
    {
        private List<VisualElement> _visualElements = new();

        public async UniTask Hide(VisualElement element)
        {
            var tcs = new UniTaskCompletionSource();

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                if (evt.stylePropertyNames.Contains("opacity"))
                {
                    element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                    tcs.TrySetResult();
                }
            }

            element.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            element.schedule.Execute(() => element.AddToClassList("hide"));

            await tcs.Task;
        }

        public async UniTask Show(VisualElement element)
        {
            var tcs = new UniTaskCompletionSource();

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                if (evt.stylePropertyNames.Contains("opacity"))
                {
                    element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                    tcs.TrySetResult();
                }
            }

            element.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            element.schedule.Execute(() => element.RemoveFromClassList("hide"));

            await tcs.Task;
        }

        public void MoveToScreenToPos(Vector2 screenPos, VisualElement root, VisualElement movementElement)
        {
            var localPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
            var clampedX = Mathf.Clamp(localPos.x, 0, root.resolvedStyle.width - movementElement.resolvedStyle.width);
            var clampedY = Mathf.Clamp(localPos.y, 0, root.resolvedStyle.height - movementElement.resolvedStyle.height);

            movementElement.style.left = clampedX;
            movementElement.style.top = clampedY;
        }

        public bool IsPointerOverUI(Vector2 screenPos, VisualElement element)
        {
            _visualElements.Clear();

            var panel = element.panel;

            if (panel == null)
                return false;

            screenPos.y = Screen.height - screenPos.y;
            panel.PickAll(screenPos, _visualElements);

            foreach (var el in _visualElements)
            {
                if (el.pickingMode == PickingMode.Position)
                    return true;
            }

            return false;
        }
    }
}