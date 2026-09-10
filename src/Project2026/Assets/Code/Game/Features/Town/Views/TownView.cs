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
            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].sprite = _viewsData[level].Sprites[i];
            }
        }
    }

    [Serializable]
    public class TownViewData
    {
        [field: SerializeField] public Sprite[] Sprites { get; private set; }
    }
}