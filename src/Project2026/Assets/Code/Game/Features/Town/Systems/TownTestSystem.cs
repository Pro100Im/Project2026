using Entitas;
using UnityEngine;

namespace Code.Game.Features.Town.Systems
{
    public class TownTestSystem : IExecuteSystem
    {
        public void Execute()
        {
            Debug.LogWarning("TownTestSystem executed");
        }
    }
}