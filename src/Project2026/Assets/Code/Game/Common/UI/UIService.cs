using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common.UI
{
    public class UIService
    {
        private List<VisualElement> _visualElements = new();

        private Dictionary<VisualElement, CancellationTokenSource> _activeTransitions = new();

        public async UniTask Hide(VisualElement element)
        {
            if (element.ClassListContains("hide")) 
                return;

            await PlayTransition(element, true);
        }

        public async UniTask Show(VisualElement element)
        {
            if (!element.ClassListContains("hide")) 
                return;

            await PlayTransition(element, false);
        }

        private async UniTask PlayTransition(VisualElement element, bool isHiding)
        {
            if (_activeTransitions.TryGetValue(element, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            var tcs = new UniTaskCompletionSource();

            _activeTransitions[element] = cts;

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                if (evt.stylePropertyNames.Contains("opacity"))
                {
                    element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                    tcs.TrySetResult();
                }
            }

            element.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

            element.schedule.Execute(() =>
            {
                if (isHiding) 
                    element.AddToClassList("hide");
                else 
                    element.RemoveFromClassList("hide");
            });

            try
            {
                await tcs.Task.AttachExternalCancellation(cts.Token).Timeout(TimeSpan.FromSeconds(0.5f));
            }
            catch (OperationCanceledException)
            {
                element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
            catch (TimeoutException)
            {
                element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
            finally
            {
                if (_activeTransitions.TryGetValue(element, out var currentCts) && currentCts == cts)
                {
                    _activeTransitions.Remove(element);

                    cts.Dispose();
                }
            }
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