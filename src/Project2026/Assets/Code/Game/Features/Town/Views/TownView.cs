using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Town.Views
{
    public class TownView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _views;
        [SerializeField] private TownViewData[] _viewsData;

        public void SetViews(int level)
        {
            if (level < 0 || level >= _viewsData.Length)
                return;

            var sprites = _viewsData[level].Sprites;
            var count = Mathf.Min(_views.Length, sprites.Length);

            for (var i = 0; i < count; i++)
                _views[i].sprite = sprites[i];
        }

        public void SetViews(int level, List<Transform> changed)
        {
            if (level < 0 || level >= _viewsData.Length)
                return;

            var sprites = _viewsData[level].Sprites;
            var count = Mathf.Min(_views.Length, sprites.Length);

            for (var i = 0; i < count; i++)
            {
                var view = _views[i];
                var sprite = sprites[i];

                if (view.sprite == sprite)
                    continue;

                view.sprite = sprite;
                changed.Add(view.transform);
            }
        }
    }

    [Serializable]
    public class TownViewData
    {
        [field: SerializeField] public Sprite[] Sprites { get; private set; }
    }
}
