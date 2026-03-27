using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Code.Common.UI
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
    }
}