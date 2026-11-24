using UnityEngine;

namespace Code.Infrastructure.Factories
{
    public interface ILevelFactory
    {
        GameObject Create(GameObject prefab);
    }
}
