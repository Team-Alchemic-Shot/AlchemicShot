using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElementImpactColorReverter
{
    private sealed class ColorRevertEntry
    {
        public Color Color;
        public float Duration;
    }

    private sealed class ColorRevertState
    {
        public readonly List<ColorRevertEntry> Entries = new();
        public Color OriginalColor;
        public Coroutine Runner;
        public MonoBehaviour RunnerHost;
    }

    private static readonly Dictionary<MeshRenderer, ColorRevertState> ColorStates = new();

    public static void ApplyColor(GameObject target, MeshRenderer renderer, Color color, float duration)
    {
        if (target == null || renderer == null)
        {
            return;
        }

        if (!ColorStates.TryGetValue(renderer, out var state))
        {
            state = new ColorRevertState
            {
                OriginalColor = renderer.material.color,
            };
            ColorStates[renderer] = state;
        }

        renderer.material.color = color;

        state.Entries.Add(new ColorRevertEntry
        {
            Color = color,
            Duration = Mathf.Max(0.05f, duration),
        });

        if (state.RunnerHost == null && target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            state.RunnerHost = monoBehaviour;
        }

        if (state.RunnerHost != null)
        {
            if (state.Runner != null)
            {
                state.RunnerHost.StopCoroutine(state.Runner);
            }
            state.Runner = state.RunnerHost.StartCoroutine(RevertMeshColorAfterDelay(renderer));
        }
    }

    private static IEnumerator RevertMeshColorAfterDelay(MeshRenderer renderer)
    {
        while (renderer != null)
        {
            if (!ColorStates.TryGetValue(renderer, out var state))
            {
                yield break;
            }

            if (state.Entries.Count == 0)
            {
                renderer.material.color = state.OriginalColor;
                ColorStates.Remove(renderer);
                state.Runner = null;
                state.RunnerHost = null;
                yield break;
            }

            var latest = state.Entries[state.Entries.Count - 1];
            renderer.material.color = latest.Color;
            yield return new WaitForSeconds(latest.Duration);

            if (ColorStates.TryGetValue(renderer, out var currentState))
            {
                if (currentState.Entries.Count > 0 && currentState.Entries[currentState.Entries.Count - 1] == latest)
                {
                    currentState.Entries.RemoveAt(currentState.Entries.Count - 1);
                }
            }
        }
    }
}
