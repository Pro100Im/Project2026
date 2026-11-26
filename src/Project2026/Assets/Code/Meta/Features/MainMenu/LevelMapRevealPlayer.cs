using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.UI.MainMenu
{
    /// <summary>
    /// Tiny timeline for the world map reveal: tracks with a delay and a duration get their 0..1
    /// progress every frame. Cosmetic only, the map state is already final when it plays.
    /// </summary>
    public sealed class LevelMapRevealPlayer
    {
        private readonly List<Track> _tracks = new(32);
        private readonly IVisualElementScheduledItem _tick;
        private float _start;

        public LevelMapRevealPlayer(VisualElement host)
        {
            _tick = host.schedule.Execute(Tick).Every(16);
            _tick.Pause();
        }

        public bool Playing => _tracks.Count > 0;

        /// <summary>Adds a track; <paramref name="update"/> is called with 0 right away.</summary>
        public void Add(float delay, float duration, Action<float> update, Action done = null)
        {
            update?.Invoke(0f);
            _tracks.Add(new Track(delay, Mathf.Max(0.0001f, duration), update, done));
        }

        /// <summary>Runs <paramref name="action"/> once at <paramref name="delay"/>.</summary>
        public void At(float delay, Action action) => _tracks.Add(new Track(delay, 0.0001f, null, action));

        public void Play()
        {
            _start = Time.unscaledTime;

            if (_tracks.Count > 0)
                _tick.Resume();
        }

        /// <summary>Jumps every track to its end state.</summary>
        public void Complete()
        {
            _tick.Pause();

            // Done callbacks may add nothing new, but iterate over a copy to be safe.
            var tracks = _tracks.ToArray();
            _tracks.Clear();

            for (var i = 0; i < tracks.Length; i++)
                tracks[i].Finish();
        }

        private void Tick(TimerState timer)
        {
            var time = Time.unscaledTime - _start;

            for (var i = _tracks.Count - 1; i >= 0; i--)
            {
                var track = _tracks[i];

                if (time < track.Delay)
                    continue;

                var progress = Mathf.Clamp01((time - track.Delay) / track.Duration);
                track.Update?.Invoke(progress);

                if (progress < 1f)
                    continue;

                _tracks.RemoveAt(i);
                track.Done?.Invoke();
            }

            if (_tracks.Count == 0)
                _tick.Pause();
        }

        public static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        public static float EaseInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;

        // Overshoots a little past 1 and settles: a "pop".
        public static float BackOut(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        private sealed class Track
        {
            public readonly float Delay;
            public readonly float Duration;
            public readonly Action<float> Update;
            public readonly Action Done;

            public Track(float delay, float duration, Action<float> update, Action done)
            {
                Delay = delay;
                Duration = duration;
                Update = update;
                Done = done;
            }

            public void Finish()
            {
                Update?.Invoke(1f);
                Done?.Invoke();
            }
        }
    }
}
