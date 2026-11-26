using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.UI.MainMenu
{
    public readonly struct LevelMapLink
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly bool Passed;
        public readonly bool Fresh;

        public LevelMapLink(Vector2 from, Vector2 to, bool passed, bool fresh = false)
        {
            From = from;
            To = to;
            Passed = passed;
            Fresh = fresh;
        }
    }

    /// <summary>
    /// A level flag on the world map: earned stars above, a banner on a pole, the title under the base.
    /// The element's pivot point (pole base) is <see cref="Anchor"/> from its top-left corner.
    /// </summary>
    public sealed class LevelMapMarker : VisualElement
    {
        public const float Width = 96f;
        // Stars row (24) + flag pole base (flag height 104 - 6).
        public static readonly Vector2 Anchor = new(Width * 0.5f, 122f);

        private const string LockedClass = "level-marker--locked";

        private readonly LevelMapStar[] _stars = new LevelMapStar[3];
        private readonly LevelMapFlag _flag;
        private readonly Label _title;

        public LevelMapMarker(Action onClick)
        {
            AddToClassList("level-marker");

            var stars = new VisualElement { pickingMode = PickingMode.Ignore };
            stars.AddToClassList("level-marker__stars");
            Add(stars);

            for (var i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new LevelMapStar();
                _stars[i].AddToClassList("level-marker__star");
                stars.Add(_stars[i]);
            }

            _flag = new LevelMapFlag { pickingMode = PickingMode.Ignore };
            _flag.AddToClassList("level-marker__flag");
            Add(_flag);

            _title = new Label { pickingMode = PickingMode.Ignore };
            _title.AddToClassList("commonText");
            _title.AddToClassList("level-marker__title");
            Add(_title);

            this.AddManipulator(new Clickable(onClick));

            // Pops and shakes pivot on the pole base, not the element center.
            style.transformOrigin = new TransformOrigin(Anchor.x, Anchor.y);
            _flag.style.transformOrigin = new TransformOrigin(Length.Percent(50f), Length.Percent(100f));
        }

        /// <summary>Reveal animation: 0 = invisible and small, 1 = normal.</summary>
        public void SetAppear(float opacity, float scale)
        {
            style.opacity = opacity;
            style.scale = new Scale(new Vector2(scale, scale));
        }

        /// <summary>Unlock animation: the banner wobbles on its pole.</summary>
        public void SetShake(float degrees) => _flag.style.rotate = new Rotate(new Angle(degrees, AngleUnit.Degree));

        /// <summary>Blocks clicks while an animation is playing; <see cref="Set"/> restores it.</summary>
        public void SetAnimating() => pickingMode = PickingMode.Ignore;

        public void Set(string title, LevelNodeState state, int stars)
        {
            _title.text = title;
            style.display = state == LevelNodeState.Hidden ? DisplayStyle.None : DisplayStyle.Flex;

            // The next level is shown but can't be picked until a road to it is open.
            var locked = state == LevelNodeState.Locked;
            pickingMode = locked ? PickingMode.Ignore : PickingMode.Position;
            EnableInClassList(LockedClass, locked);

            var completed = state == LevelNodeState.Completed;
            _flag.SetState(state);

            for (var i = 0; i < _stars.Length; i++)
            {
                _stars[i].style.visibility = completed ? Visibility.Visible : Visibility.Hidden;
                _stars[i].SetFilled(i < stars);
            }
        }

        public void PlaceAt(Vector2 point)
        {
            style.left = point.x - Anchor.x;
            style.top = point.y - Anchor.y;
        }
    }

    /// <summary>Pole with a shield banner, drawn procedurally so the map works without art.</summary>
    public sealed class LevelMapFlag : VisualElement
    {
        private static readonly Color Pole = new(0.30f, 0.19f, 0.10f);
        private static readonly Color Shadow = new(0f, 0f, 0f, 0.25f);
        private static readonly Color Outline = new(0.18f, 0.12f, 0.08f);
        private static readonly Color OpenBanner = new(0.80f, 0.20f, 0.16f);
        private static readonly Color OpenTrim = new(0.98f, 0.86f, 0.55f);
        private static readonly Color DoneBanner = new(0.70f, 0.72f, 0.76f);
        private static readonly Color DoneTrim = new(0.96f, 0.78f, 0.30f);
        private static readonly Color LockedPole = new(0.32f, 0.29f, 0.27f);
        private static readonly Color LockedBanner = new(0.40f, 0.44f, 0.52f);
        private static readonly Color LockedTrim = new(0.66f, 0.68f, 0.72f);
        private static readonly Color Emblem = new(1f, 1f, 1f, 0.85f);

        private LevelNodeState _state = LevelNodeState.Available;

        public LevelMapFlag()
        {
            generateVisualContent += Draw;
        }

        public void SetState(LevelNodeState state)
        {
            if (_state == state)
                return;

            _state = state;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var p = context.painter2D;
            var w = contentRect.width;
            var h = contentRect.height;
            var cx = w * 0.5f;
            var baseY = h - 6f;
            var completed = _state == LevelNodeState.Completed;
            var locked = _state == LevelNodeState.Locked;
            var banner = locked ? LockedBanner : completed ? DoneBanner : OpenBanner;
            var trim = locked ? LockedTrim : completed ? DoneTrim : OpenTrim;

            // Ground shadow.
            p.fillColor = Shadow;
            Ellipse(p, new Vector2(cx, baseY), 26f, 7f);
            p.Fill();

            // Pole.
            p.fillColor = locked ? LockedPole : Pole;
            Rect(p, new Rect(cx - 3f, 8f, 6f, baseY - 8f));
            p.Fill();
            p.fillColor = trim;
            Ellipse(p, new Vector2(cx, 8f), 6f, 6f);
            p.Fill();

            // Shield banner hanging from the pole top.
            var top = 16f;
            var bw = 64f;
            var bh = 66f;
            var left = cx - bw * 0.5f;

            p.BeginPath();
            p.MoveTo(new Vector2(left, top));
            p.LineTo(new Vector2(left + bw, top));
            p.LineTo(new Vector2(left + bw, top + bh * 0.62f));
            p.LineTo(new Vector2(cx, top + bh));
            p.LineTo(new Vector2(left, top + bh * 0.62f));
            p.ClosePath();
            p.fillColor = banner;
            p.Fill();
            p.strokeColor = trim;
            p.lineWidth = 4f;
            p.Stroke();
            p.strokeColor = Outline;
            p.lineWidth = 1.5f;
            p.Stroke();

            // Emblem: crossed swords for open levels, a castle tower for beaten ones, a padlock for locked.
            p.strokeColor = Emblem;
            p.fillColor = Emblem;
            p.lineWidth = 4f;
            p.lineCap = LineCap.Round;

            var ec = new Vector2(cx, top + bh * 0.42f);

            if (locked)
            {
                // Shackle.
                p.lineWidth = 3.5f;
                p.BeginPath();
                p.Arc(ec + new Vector2(0f, -3f), 7f, 180f, 360f);
                p.Stroke();

                // Body with a keyhole cut by the banner color.
                Rect(p, new Rect(ec.x - 11f, ec.y - 3f, 22f, 17f));
                p.Fill();
                p.fillColor = banner;
                Ellipse(p, ec + new Vector2(0f, 4f), 2.5f, 2.5f);
                p.Fill();
                Rect(p, new Rect(ec.x - 1.2f, ec.y + 5f, 2.4f, 5f));
                p.Fill();
            }
            else if (completed)
            {
                Rect(p, new Rect(ec.x - 11f, ec.y - 8f, 22f, 20f));
                p.Fill();

                for (var i = 0; i < 3; i++)
                {
                    Rect(p, new Rect(ec.x - 11f + i * 8f, ec.y - 14f, 6f, 6f));
                    p.Fill();
                }
            }
            else
            {
                p.BeginPath();
                p.MoveTo(ec + new Vector2(-13f, -13f));
                p.LineTo(ec + new Vector2(13f, 13f));
                p.MoveTo(ec + new Vector2(13f, -13f));
                p.LineTo(ec + new Vector2(-13f, 13f));
                p.Stroke();
            }
        }

        private static void Rect(Painter2D p, Rect r)
        {
            p.BeginPath();
            p.MoveTo(new Vector2(r.xMin, r.yMin));
            p.LineTo(new Vector2(r.xMax, r.yMin));
            p.LineTo(new Vector2(r.xMax, r.yMax));
            p.LineTo(new Vector2(r.xMin, r.yMax));
            p.ClosePath();
        }

        internal static void Ellipse(Painter2D p, Vector2 c, float rx, float ry)
        {
            const float k = 0.5523f;
            p.BeginPath();
            p.MoveTo(new Vector2(c.x + rx, c.y));
            p.BezierCurveTo(new Vector2(c.x + rx, c.y + ry * k), new Vector2(c.x + rx * k, c.y + ry), new Vector2(c.x, c.y + ry));
            p.BezierCurveTo(new Vector2(c.x - rx * k, c.y + ry), new Vector2(c.x - rx, c.y + ry * k), new Vector2(c.x - rx, c.y));
            p.BezierCurveTo(new Vector2(c.x - rx, c.y - ry * k), new Vector2(c.x - rx * k, c.y - ry), new Vector2(c.x, c.y - ry));
            p.BezierCurveTo(new Vector2(c.x + rx * k, c.y - ry), new Vector2(c.x + rx, c.y - ry * k), new Vector2(c.x + rx, c.y));
            p.ClosePath();
        }
    }

    /// <summary>Five-point star, gold when earned and dark when not.</summary>
    public sealed class LevelMapStar : VisualElement
    {
        private static readonly Color Filled = new(1f, 0.82f, 0.22f);
        private static readonly Color Empty = new(0.25f, 0.20f, 0.16f, 0.85f);
        private static readonly Color Outline = new(0.35f, 0.20f, 0.05f);

        private bool _filled = true;

        public LevelMapStar()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public void SetFilled(bool filled)
        {
            if (_filled == filled)
                return;

            _filled = filled;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var p = context.painter2D;
            var r = contentRect;
            var c = r.center;
            var outer = Mathf.Min(r.width, r.height) * 0.5f - 1f;
            var inner = outer * 0.45f;

            p.BeginPath();

            for (var i = 0; i < 10; i++)
            {
                var radius = i % 2 == 0 ? outer : inner;
                var angle = -Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                var point = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (i == 0)
                    p.MoveTo(point);
                else
                    p.LineTo(point);
            }

            p.ClosePath();
            p.fillColor = _filled ? Filled : Empty;
            p.Fill();
            p.strokeColor = Outline;
            p.lineWidth = 1.5f;
            p.Stroke();
        }
    }

    /// <summary>Dashed, slightly curved roads between level flags.</summary>
    public sealed class LevelMapRoads : VisualElement
    {
        private const float Dash = 14f;
        private const float Gap = 10f;
        public const float DefaultBend = 0.18f;
        private const int Samples = 48;

        /// <summary>Sideways bend of a road, fraction of its length.</summary>
        public float Bend = DefaultBend;
        public float LineWidth = 6f;
        public Color Passed = new(0.28f, 0.18f, 0.10f, 0.95f);
        public Color Fresh = new(0.28f, 0.18f, 0.10f, 0.6f);

        private readonly List<LevelMapLink> _links = new(16);
        private readonly List<float> _progress = new(16);
        private readonly Vector2[] _points = new Vector2[Samples + 1];

        public LevelMapRoads()
        {
            pickingMode = PickingMode.Ignore;
            // Fills its parent; set here so the element works in any screen without extra USS.
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            generateVisualContent += Draw;
        }

        public void Set(List<LevelMapLink> links)
        {
            _links.Clear();
            _links.AddRange(links);
            _progress.Clear();

            for (var i = 0; i < links.Count; i++)
                _progress.Add(1f);

            MarkDirtyRepaint();
        }

        /// <summary>How much of a road is drawn, from its start (0..1). Used to draw a new road in.</summary>
        public void SetProgress(int index, float progress)
        {
            if (index < 0 || index >= _progress.Count)
                return;

            _progress[index] = Mathf.Clamp01(progress);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var p = context.painter2D;
            p.lineWidth = LineWidth;
            p.lineCap = LineCap.Round;

            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                if (_progress[i] <= 0f)
                    continue;

                // The partial curve is the same geometry, so dashes stay put while the road grows.
                p.strokeColor = link.Passed ? Passed : Fresh;
                Sample(link.From, link.To, _progress[i]);
                StrokeDashed(p);
            }
        }

        // Samples the first part of the curve, up to 'until'.
        private void Sample(Vector2 from, Vector2 to, float until)
        {
            for (var i = 0; i <= Samples; i++)
                _points[i] = Point(from, to, until * i / Samples, Bend);
        }

        /// <summary>A point on the road curve: quadratic, bent sideways, always to the same side for a given direction.</summary>
        public static Vector2 Point(Vector2 from, Vector2 to, float t, float bend = DefaultBend)
        {
            var delta = to - from;
            var normal = new Vector2(-delta.y, delta.x).normalized;
            var control = (from + to) * 0.5f + normal * delta.magnitude * bend;
            var u = 1f - t;
            return u * u * from + 2f * u * t * control + t * t * to;
        }

        private void StrokeDashed(Painter2D p)
        {
            var drawing = true;
            var left = Dash;

            p.BeginPath();
            p.MoveTo(_points[0]);

            for (var i = 1; i < _points.Length; i++)
            {
                var a = _points[i - 1];
                var b = _points[i];
                var length = Vector2.Distance(a, b);
                var travelled = 0f;

                while (length - travelled > left)
                {
                    travelled += left;
                    var point = Vector2.Lerp(a, b, travelled / length);

                    if (drawing)
                        p.LineTo(point);
                    else
                        p.MoveTo(point);

                    drawing = !drawing;
                    left = drawing ? Dash : Gap;
                }

                left -= length - travelled;

                if (drawing)
                    p.LineTo(b);
                else
                    p.MoveTo(b);
            }

            p.Stroke();
        }
    }

    /// <summary>
    /// Map backdrop. Uses the configured sprite, or paints a parchment placeholder with a few hills.
    /// </summary>
    public sealed class LevelMapBackdrop : VisualElement
    {
        private static readonly Color Parchment = new(0.86f, 0.75f, 0.53f);
        private static readonly Color Edge = new(0.45f, 0.32f, 0.18f);
        private static readonly Color Ink = new(0.48f, 0.36f, 0.22f, 0.35f);

        private bool _placeholder = true;

        public LevelMapBackdrop()
        {
            pickingMode = PickingMode.Ignore;
            // Fills its parent; set here so the element works in any screen without extra USS.
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            generateVisualContent += Draw;
        }

        public void SetSprite(Sprite sprite)
        {
            _placeholder = sprite == null;
            style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            if (!_placeholder)
                return;

            var p = context.painter2D;
            var r = contentRect;

            if (r.width <= 0f || r.height <= 0f)
                return;

            p.fillColor = Parchment;
            p.BeginPath();
            p.MoveTo(new Vector2(r.xMin, r.yMin));
            p.LineTo(new Vector2(r.xMax, r.yMin));
            p.LineTo(new Vector2(r.xMax, r.yMax));
            p.LineTo(new Vector2(r.xMin, r.yMax));
            p.ClosePath();
            p.Fill();

            // Hand-drawn style hills, deterministic so the map does not change between openings.
            var random = new System.Random(2026);
            p.strokeColor = Ink;
            p.lineWidth = 3f;
            p.lineJoin = LineJoin.Round;
            var count = Mathf.RoundToInt(r.width * r.height / 40000f);

            for (var i = 0; i < count; i++)
            {
                var x = (float)random.NextDouble() * r.width;
                var y = (float)random.NextDouble() * r.height;
                var size = 18f + (float)random.NextDouble() * 26f;

                p.BeginPath();
                p.MoveTo(new Vector2(x - size, y));
                p.LineTo(new Vector2(x - size * 0.2f, y - size * 0.9f));
                p.LineTo(new Vector2(x + size * 0.15f, y - size * 0.45f));
                p.LineTo(new Vector2(x + size * 0.45f, y - size * 0.7f));
                p.LineTo(new Vector2(x + size, y));
                p.Stroke();
            }

            // Darkened edges.
            p.lineWidth = 18f;

            for (var i = 0; i < 4; i++)
            {
                var inset = 9f + i * 16f;
                p.strokeColor = new Color(Edge.r, Edge.g, Edge.b, 0.45f - i * 0.1f);
                p.BeginPath();
                p.MoveTo(new Vector2(r.xMin + inset, r.yMin + inset));
                p.LineTo(new Vector2(r.xMax - inset, r.yMin + inset));
                p.LineTo(new Vector2(r.xMax - inset, r.yMax - inset));
                p.LineTo(new Vector2(r.xMin + inset, r.yMax - inset));
                p.ClosePath();
                p.Stroke();
            }
        }
    }
}
