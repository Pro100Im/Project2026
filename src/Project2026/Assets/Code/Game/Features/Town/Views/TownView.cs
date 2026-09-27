using System;
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
    }

    [Serializable]
    public class TownViewData
    {
        [field: SerializeField] public Sprite[] Sprites { get; private set; }
    }
}