using Code.Game.StaticData.Data;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Waves/WavesConfig")]
    public class WavesConfig : ScriptableObject
    {
        [field: SerializeField] public WaveData[] WaveDatas { get; private set; }
    }
}