// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause
using System.Collections;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.UX.Experimental
{
    public class NonNativeKeyTouchAdapter : MonoBehaviour
    {
        private const float ColliderMargin = 30.0f;
        private const float ColliderThickness = 70.0f;
        private const float ColliderZDelta = 20.0f;
        private const float ReClickDelayTime = 1.0f;
        private const float AnimationTime = 0.2f;
        private const float AnimationMovementDelta = 20.0f;

        private StatefulInteractable interactable;
        private Graphic image;
        private float lastClickTime;
        private bool isInitialized;
        private Vector3 defaultPosition;
        private Vector3 animatedPosition;
        private BoxCollider buttonCollider;
        private Vector3 buttonColliderDefaultCenter;
        private Color defaultImageColor;

        private void Awake()
        {
            defaultPosition = transform.localPosition;
            animatedPosition = defaultPosition + new Vector3(0, 0, AnimationMovementDelta);
        }

        private void OnEnable()
        {
            transform.localPosition = defaultPosition;
            lastClickTime = Time.time;
            if (isInitialized)
            {
                 buttonCollider.center = buttonColliderDefaultCenter;
            }

            Initialize();
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
            
            image = GetComponent<Graphic>();
            defaultImageColor = image.color;
            var button = GetComponent<Button>();
            
            interactable = gameObject.EnsureComponent<StatefulInteractable>();
            interactable.firstSelectEntered.AddListener(selectArgs =>
            {
                if (selectArgs.interactorObject is not PokeInteractor ||
                    Time.time - lastClickTime < ReClickDelayTime)
                {
                    return;
                }

                button.onClick.Invoke();
                StartCoroutine(MoveButton(defaultPosition, animatedPosition));
            });
            button.interactable = false;

            interactable.lastSelectExited.AddListener(_ =>
            {
                StartCoroutine(MoveButton(animatedPosition, defaultPosition));
            });

            interactable.firstHoverEntered.AddListener(hoverArgs =>
            {
                SetColorOnHoverPoke(hoverArgs.interactorObject, button.colors.highlightedColor);
            });
            
            interactable.lastHoverExited.AddListener(hoverArgs =>
            {
                SetColorOnHoverPoke(hoverArgs.interactorObject, defaultImageColor);
            });
        }

        private void SetColorOnHoverPoke(IXRHoverInteractor interaction, Color color)
        {
            if (interaction is PokeInteractor)
            {
                image.color = color;
            }
        }
        
        private void Update()
        {
            // fallback for if the user very rapidly moves their finger over the keyboard and
            // interactable.lastHoverExited does not get fired (or too early)
            if (interactable.HoveringPokeInteractors.Count == 0 && image.color != defaultImageColor)
            {
                image.color = defaultImageColor;
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
                transform.localPosition = newPos; buttonCollider.center = buttonColliderDefaultCenter - (newPos - defaultPosition);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            interactable.hoverExited.RemoveAllListeners();
            interactable.hoverEntered.RemoveAllListeners();
            interactable.firstSelectEntered.RemoveAllListeners();
            interactable.lastSelectExited.RemoveAllListeners();
        }
    }
}