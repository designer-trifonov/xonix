using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HippoGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PanelAspectFitter : MonoBehaviour
    {
        [Range(0f, 0.49f)] [SerializeField] private float _paddingXPercent = 0.05f;
        [Range(0f, 0.49f)] [SerializeField] private float _paddingYPercent = 0.05f;

        private RectTransform _rt;
        private int           _lastWidth;
        private int           _lastHeight;

        private const float Aspect = 4f / 3f;

        private void Awake()    => _rt = GetComponent<RectTransform>();
        private void OnEnable() => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            yield return null;
            yield return null;
            Apply();
        }

        private void Update()
        {
            if (Screen.width == _lastWidth && Screen.height == _lastHeight) return;
            _lastWidth  = Screen.width;
            _lastHeight = Screen.height;
            Apply();
        }

        private void Apply()
        {
            if (_rt == null) return;

            Canvas rootCanvas = GetRootCanvas();
            CanvasScaler scaler = rootCanvas != null ? rootCanvas.GetComponent<CanvasScaler>() : null;

            float canvasW, canvasH;

            if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                float refW = scaler.referenceResolution.x;
                float refH = scaler.referenceResolution.y;
                float screenAspect = (float)Screen.width / Screen.height;

                if (scaler.matchWidthOrHeight < 0.001f)
                {
                    canvasW = refW;
                    canvasH = refW / screenAspect;
                }
                else if (scaler.matchWidthOrHeight > 0.999f)
                {
                    canvasH = refH;
                    canvasW = refH * screenAspect;
                }
                else
                {
                    float logW = Mathf.Log(Screen.width  / refW, 2f);
                    float logH = Mathf.Log(Screen.height / refH, 2f);
                    float logScale = Mathf.Lerp(logW, logH, scaler.matchWidthOrHeight);
                    float scale = Mathf.Pow(2f, logScale);
                    canvasW = Screen.width  / scale;
                    canvasH = Screen.height / scale;
                }
            }
            else
            {
                RectTransform parentRT = _rt.parent as RectTransform;
                if (parentRT != null)
                {
                    canvasW = parentRT.rect.width;
                    canvasH = parentRT.rect.height;
                }
                else
                {
                    canvasW = Screen.width;
                    canvasH = Screen.height;
                }
            }

            if (canvasW <= 0f || canvasH <= 0f) return;

            float maxW = canvasW * (1f - _paddingXPercent * 2f);
            float maxH = canvasH * (1f - _paddingYPercent * 2f);

            float w = maxH * Aspect;
            float h = maxH;
            if (w > maxW) { w = maxW; h = maxW / Aspect; }

            _rt.anchorMin        = new Vector2(0.5f, 0.5f);
            _rt.anchorMax        = new Vector2(0.5f, 0.5f);
            _rt.pivot            = new Vector2(0.5f, 0.5f);
            _rt.anchoredPosition = Vector2.zero;
            _rt.sizeDelta        = new Vector2(w, h);
        }

        private Canvas GetRootCanvas()
        {
            Transform t = transform;
            Canvas found = null;
            while (t != null)
            {
                var c = t.GetComponent<Canvas>();
                if (c != null) found = c;
                t = t.parent;
            }
            return found;
        }
    }
}
