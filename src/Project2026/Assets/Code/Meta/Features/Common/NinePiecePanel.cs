using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.UI.Common
{
    /// <summary>
    /// Background built from the Tiny Swords pack's panel sheets, which ship as 9 separate pieces
    /// (corners, edges, center) rather than one 9-slice sprite. Corners keep their size, edges and
    /// the center stretch. The pieces come from USS custom properties <c>--nine-0 .. --nine-8</c>
    /// (row by row from the top-left) and <c>--nine-scale</c>. With only <c>--nine-0..2</c> set it is
    /// a horizontal 3-piece strip (ribbons). Fills its parent and ignores the pointer.
    /// </summary>
    [UxmlElement]
    public partial class NinePiecePanel : VisualElement
    {
        public const string PieceClass = "nine__piece";

        private const int Count = 9;

        // Every piece in the pack's sheets has a 1px soft fringe on the sides that meet its neighbours
        // (the pieces are 66px in a 64px grid). Stretching a piece stretches that fringe into a wide
        // see-through stripe, so stretched pieces are drawn from a copy of the sprite without it.
        private const int Fringe = 1;

        // Stretched pieces slide under the corners, which hides the corners' own (unstretched) fringe.
        private const float Overlap = 2f;

        private static readonly Dictionary<(Sprite, bool, bool), Sprite> Trimmed = new();

        private static readonly CustomStyleProperty<Sprite>[] SpriteProperties = CreateProperties();
        private static readonly CustomStyleProperty<float> ScaleProperty = new("--nine-scale");

        private readonly VisualElement[] _pieces = new VisualElement[Count];
        private readonly Sprite[] _sprites = new Sprite[Count];
        private float _scale = 1f;

        public NinePiecePanel()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;

            // Center and edges first, corners on top.
            var order = new[] { 4, 1, 3, 5, 7, 0, 2, 6, 8 };

            for (var i = 0; i < order.Length; i++)
            {
                var piece = new VisualElement { pickingMode = PickingMode.Ignore };
                piece.AddToClassList(PieceClass);
                piece.style.position = Position.Absolute;
                piece.style.backgroundSize = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));
                _pieces[order[i]] = piece;
                Add(piece);
            }

            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
        }

        private static CustomStyleProperty<Sprite>[] CreateProperties()
        {
            var properties = new CustomStyleProperty<Sprite>[Count];

            for (var i = 0; i < Count; i++)
                properties[i] = new CustomStyleProperty<Sprite>("--nine-" + i);

            return properties;
        }

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            var changed = false;

            for (var i = 0; i < Count; i++)
            {
                evt.customStyle.TryGetValue(SpriteProperties[i], out var sprite);

                if (_sprites[i] == sprite)
                    continue;

                _sprites[i] = sprite;
                changed = true;
            }

            var scale = evt.customStyle.TryGetValue(ScaleProperty, out var value) && value > 0f ? value : 1f;

            if (!Mathf.Approximately(scale, _scale))
            {
                _scale = scale;
                changed = true;
            }

            if (changed)
                Rebuild();
        }

        private void Rebuild()
        {
            for (var i = 0; i < Count; i++)
            {
                var sprite = Trim(_sprites[i], i, _sprites[3] == null);
                _pieces[i].style.display = sprite != null ? DisplayStyle.Flex : DisplayStyle.None;
                _pieces[i].style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
            }

            if (_sprites[0] == null || _sprites[1] == null || _sprites[2] == null)
                return;

            if (_sprites[3] == null)
                LayoutStrip();
            else
                LayoutGrid();
        }

        // The sprite a piece is drawn with: cut the fringe off on the axes the piece is stretched along.
        private static Sprite Trim(Sprite sprite, int index, bool strip)
        {
            if (sprite == null)
                return null;

            var horizontal = index % 3 == 1;
            var vertical = !strip && index / 3 == 1;

            if (!horizontal && !vertical)
                return sprite;

            var key = (sprite, horizontal, vertical);

            if (Trimmed.TryGetValue(key, out var trimmed) && trimmed != null)
                return trimmed;

            var rect = sprite.rect;

            if (horizontal)
            {
                rect.xMin += Fringe;
                rect.xMax -= Fringe;
            }

            if (vertical)
            {
                rect.yMin += Fringe;
                rect.yMax -= Fringe;
            }

            trimmed = Sprite.Create(sprite.texture, rect, new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            trimmed.name = sprite.name + "_trimmed";
            trimmed.hideFlags = HideFlags.HideAndDontSave;
            Trimmed[key] = trimmed;
            return trimmed;
        }

        // Rows and columns line up on their inner edges, whatever the outer overhang of each piece.
        private void LayoutGrid()
        {
            var left = Mathf.Max(Width(0), Width(3), Width(6));
            var right = Mathf.Max(Width(2), Width(5), Width(8));
            var top = Mathf.Max(Height(0), Height(1), Height(2));
            var bottom = Mathf.Max(Height(6), Height(7), Height(8));

            for (var i = 0; i < Count; i++)
            {
                if (_sprites[i] == null)
                    continue;

                var piece = _pieces[i].style;
                var column = i % 3;
                var row = i / 3;

                piece.left = piece.right = piece.top = piece.bottom = StyleKeyword.Auto;
                piece.width = piece.height = StyleKeyword.Auto;

                if (column == 0)
                {
                    piece.left = left - Width(i);
                    piece.width = Width(i);
                }
                else if (column == 2)
                {
                    piece.right = right - Width(i);
                    piece.width = Width(i);
                }
                else
                {
                    piece.left = left - Overlap;
                    piece.right = right - Overlap;
                }

                if (row == 0)
                {
                    piece.top = top - Height(i);
                    piece.height = Height(i);
                }
                else if (row == 2)
                {
                    piece.bottom = bottom - Height(i);
                    piece.height = Height(i);
                }
                else
                {
                    piece.top = top - Overlap;
                    piece.bottom = bottom - Overlap;
                }
            }
        }

        // Ends keep their size, the middle stretches; pieces keep the vertical offsets they have in the sheet.
        private void LayoutStrip()
        {
            var sheetTop = Mathf.Max(Top(0), Top(1), Top(2));

            for (var i = 0; i < 3; i++)
            {
                var piece = _pieces[i].style;
                piece.left = piece.right = piece.bottom = StyleKeyword.Auto;
                piece.width = StyleKeyword.Auto;
                piece.top = (sheetTop - Top(i)) * _scale;
                piece.height = Height(i);
            }

            _pieces[0].style.left = 0f;
            _pieces[0].style.width = Width(0);
            _pieces[2].style.right = 0f;
            _pieces[2].style.width = Width(2);
            _pieces[1].style.left = Width(0) - Overlap;
            _pieces[1].style.right = Width(2) - Overlap;
        }

        private float Width(int index) => _sprites[index] != null ? _sprites[index].rect.width * _scale : 0f;

        private float Height(int index) => _sprites[index] != null ? _sprites[index].rect.height * _scale : 0f;

        private float Top(int index) => _sprites[index] != null ? _sprites[index].rect.yMax : 0f;
    }
}
