using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common.UI
{
    public class UIService
    {
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

        public bool IsPointerOverUI(Vector2 screenPos, VisualElement element)
        {
            var panel = element.panel;

            if (panel == null) 
                return false;

            var isPointer = panel.Pick(screenPos);

            return isPointer != null && element.pickingMode == PickingMode.Position;
        }
    }
}