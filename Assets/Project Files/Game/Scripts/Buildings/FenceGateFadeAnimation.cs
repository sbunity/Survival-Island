using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class FenceGateFadeAnimation : FenceGateAnimation
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        [SerializeField] Material transparentMaterial;

        [SerializeField, Range(0f, 1f)] float openAlpha = 0.35f;

        [Space]
        [SerializeField, Min(0.05f)] float toTransparentDuration = 0.25f;
        [SerializeField, Min(0.05f)] float toSolidDuration = 0.3f;
        [SerializeField, Min(0f)] float staggerPerMetre = 0.05f;

        [Space]
        [SerializeField] Ease.Type toTransparentEasing = Ease.Type.QuadOut;
        [SerializeField] Ease.Type toSolidEasing = Ease.Type.QuadIn;

        private Renderer[] renderers;
        private Material[] solidMaterials;
        private Color[] baseColors;
        private float[] alphas;
        private TweenCase[] tweens;
        private MaterialPropertyBlock propertyBlock;

        public override float Duration => toTransparentDuration;

        public override void Initialise(IReadOnlyList<Transform> logs)
        {
            renderers = new Renderer[logs.Count];
            solidMaterials = new Material[logs.Count];
            baseColors = new Color[logs.Count];
            alphas = new float[logs.Count];
            tweens = new TweenCase[logs.Count];

            propertyBlock ??= new MaterialPropertyBlock();

            for (var i = 0; i < logs.Count; i++)
            {
                alphas[i] = 1f;
                baseColors[i] = Color.white;

                if (logs[i] == null)
                    continue;

                renderers[i] = logs[i].GetComponent<Renderer>();

                if (renderers[i] == null)
                    continue;

                solidMaterials[i] = renderers[i].sharedMaterial;

                if (solidMaterials[i] != null && solidMaterials[i].HasProperty(ColorPropertyId))
                    baseColors[i] = solidMaterials[i].GetColor(ColorPropertyId);
            }

            if (transparentMaterial == null)
                Debug.LogWarning("[Fence] The fade animation has no transparent material assigned - logs will stay solid.");
        }

        public override void SetLogOpen(int logIndex, bool isOpen, in FenceLogOpenContext context)
        {
            if (renderers == null || logIndex < 0 || logIndex >= renderers.Length)
                return;

            if (renderers[logIndex] == null)
                return;

            tweens[logIndex].KillActive();

            var index = logIndex;
            var target = isOpen ? openAlpha : 1f;
            var duration = isOpen ? toTransparentDuration : toSolidDuration;
            var easing = isOpen ? toTransparentEasing : toSolidEasing;
            var delay = staggerPerMetre * Mathf.Max(0f, context.DistanceFromCentre);

            if (isOpen)
                ApplyTransparentMaterial(index);

            tweens[logIndex] = this.DOAction<float>(
                (from, to, progress) => ApplyAlpha(index, Mathf.Lerp(from, to, progress)),
                alphas[logIndex],
                target,
                duration,
                delay).SetEasing(easing);

            if (!isOpen)
                tweens[logIndex].OnComplete(() => RestoreSolidMaterial(index));
        }

        public override void SnapAllClosed()
        {
            if (renderers == null)
                return;

            tweens.KillActive();

            for (var i = 0; i < renderers.Length; i++)
                RestoreSolidMaterial(i);
        }

        private void ApplyAlpha(int index, float alpha)
        {
            alphas[index] = alpha;

            var logRenderer = renderers[index];

            if (logRenderer == null)
                return;

            var color = baseColors[index];
            color.a = alpha;

            logRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorPropertyId, color);
            logRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ApplyTransparentMaterial(int index)
        {
            var logRenderer = renderers[index];

            if (logRenderer == null || transparentMaterial == null)
                return;

            if (logRenderer.sharedMaterial != transparentMaterial)
                logRenderer.sharedMaterial = transparentMaterial;
        }

        private void RestoreSolidMaterial(int index)
        {
            var logRenderer = renderers[index];

            if (logRenderer == null)
                return;

            alphas[index] = 1f;

            if (solidMaterials[index] != null && logRenderer.sharedMaterial != solidMaterials[index])
                logRenderer.sharedMaterial = solidMaterials[index];

            ApplyAlpha(index, 1f);
        }
    }
}
