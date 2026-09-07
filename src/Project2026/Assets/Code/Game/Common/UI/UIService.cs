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
        private readonly List<VisualElement> _visualElements = new();
        private readonly Stack<VisualElement> _pickingStack = new();

        private readonly Dictionary<VisualElement, CancellationTokenSource> _activeTransitions = new();

        public async UniTask Hide(VisualElement element)
        {
            SetSubtreePickingInteractable(element, false);

            if (element.ClassListContains("hide"))
                return;

            await PlayTransition(element, true);
        }

        public async UniTask Show(VisualElement element)
        {
            SetSubtreePickingInteractable(element, true);

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

        public bool HasComponent(VisualElement element, string className)
        {
            return element.ClassListContains(className);
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

            for (var i = 0; i < _visualElements.Count; i++)
            {
                var el = _visualElements[i];

                if (el.pickingMode == PickingMode.Position && !IsUnderHidden(el))
                    return true;
            }

            return false;
        }

        private static bool IsUnderHidden(VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current.ClassListContains("hide"))
                    return true;
            }

            return false;
        }

        private void SetSubtreePickingInteractable(VisualElement root, bool interactable)
        {
            _pickingStack.Clear();
            _pickingStack.Push(root);

            while (_pickingStack.Count > 0)
            {
                var element = _pickingStack.Pop();

                if (interactable)
                {
                    if (element.userData is PickingMode storedMode && !element.ClassListContains("hide"))
                    {
                        element.pickingMode = storedMode;
                        element.userData = null;
                    }
                }
                else if (element.pickingMode == PickingMode.Position)
                {
                    element.userData = PickingMode.Position;
                    element.pickingMode = PickingMode.Ignore;
                }

                var childCount = element.childCount;
                for (var i = 0; i < childCount; i++)
                    _pickingStack.Push(element[i]);
            }
        }
    }
}
