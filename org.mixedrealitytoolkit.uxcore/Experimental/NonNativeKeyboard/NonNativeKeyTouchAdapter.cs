// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.UX.Experimental
{
    /// <summary>
    /// Adds touch interaction to a button on the non-native keyboard.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Button))]
    public class NonNativeKeyTouchAdapter : MonoBehaviour
    {
        private const float ColliderMargin = 30.0f;
        private const float ColliderThickness = 70.0f;
        private const float ColliderZDelta = 20.0f;
        private const float ReClickDelayTime = 1.0f;
        private const float AnimationTime = 0.2f;
        private const float AnimationMovementDelta = 20.0f;

        private StatefulInteractable interactable;
        private float lastClickTime;
        private bool isInitialized;
        private Vector3 defaultPosition;
        private Vector3 animatedPosition;
        private BoxCollider buttonCollider;
        private Vector3 buttonColliderDefaultCenter;
        private Button button;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            defaultPosition = transform.localPosition;
            animatedPosition = defaultPosition + new Vector3(0, 0, AnimationMovementDelta);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            transform.localPosition = defaultPosition;
            lastClickTime = Time.time;
            if (isInitialized)
            {
                 buttonCollider.center = buttonColliderDefaultCenter;
            }

            Initialize();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            interactable.hoverEntered.RemoveListener(OnHoverStart);
            interactable.firstSelectEntered.RemoveListener(OnSelectStart);
            interactable.lastSelectExited.RemoveListener(OnSelectEnd);
        }

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            var rectTransform = GetComponent<RectTransform>();
            buttonCollider = gameObject.EnsureComponent<BoxCollider>();
            var size = new Vector3(
                rectTransform.rect.size.x - ColliderMargin,
                rectTransform.rect.size.y - ColliderMargin,
                ColliderThickness);
            buttonCollider.size = size;
            buttonColliderDefaultCenter = new Vector3((size.x + ColliderMargin) / 2.0f,
                (-size.y - ColliderMargin) / 2.0f, ColliderZDelta);
            buttonCollider.center = buttonColliderDefaultCenter;

            button = GetComponent<Button>();
            button.interactable = false;

            interactable = gameObject.EnsureComponent<StatefulInteractable>();
            interactable.firstSelectEntered.AddListener(OnSelectStart);
            interactable.lastSelectExited.AddListener(OnSelectEnd);
            interactable.hoverEntered.AddListener(OnHoverStart);
        }

        private void OnSelectStart(SelectEnterEventArgs selectArgs)
        {
            if (selectArgs.interactorObject is not IPokeInteractor ||
                Time.time - lastClickTime < ReClickDelayTime)
            {
                return;
            }

            button.onClick.Invoke();
            lastClickTime = Time.time;
            StartCoroutine(MoveButton(defaultPosition, animatedPosition));
        }

        private void OnSelectEnd(SelectExitEventArgs _)
        {
            StartCoroutine(MoveButton(animatedPosition, defaultPosition));
        }

        private void OnHoverStart(HoverEnterEventArgs hoverArgs)
        {
            if (hoverArgs.interactorObject is IPokeInteractor)
            {
                button.targetGraphic.CrossFadeColor(button.colors.pressedColor, button.colors.fadeDuration, true, true);
            }
        }

        private IEnumerator MoveButton(Vector3 startPos, Vector3 endPos)
        {
            if (transform.localPosition == endPos)
            {
                yield break;
            }
            const float rate = 1.0f / AnimationTime;
            var i = 0.0f;
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                var newPos = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, i));
                transform.localPosition = newPos;
                buttonCollider.center = buttonColliderDefaultCenter - (newPos - defaultPosition);
                yield return null;
            }
            if (interactable.HoveringPokeInteractors.Count == 0)
            {
                button.targetGraphic.CrossFadeColor(button.colors.normalColor, button.colors.fadeDuration, true, true);
            }
        }
    }
}
