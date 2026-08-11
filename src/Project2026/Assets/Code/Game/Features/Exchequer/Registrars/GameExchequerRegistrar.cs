using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Exchequer.Registrars
{
    public class GameExchequerRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private GameExchequerView _gameExchequerView;

        public override void RegisterComponents()
        {
            Entity.AddGameExchequer(_gameExchequerView);
            Entity.AddExchequerMealCapacity(0);
            Entity.AddExchequerManaCapacity(0);
            Entity.AddExchequerGoldCapacity(0);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasGameExchequer)
                Entity.RemoveGameExchequer();
            if (Entity.hasExchequerMealCapacity)
                Entity.RemoveExchequerMealCapacity();
            if (Entity.hasExchequerManaCapacity)
                Entity.RemoveExchequerManaCapacity();
            if (Entity.hasExchequerGoldCapacity)
                Entity.RemoveExchequerGoldCapacity();
        }
    }
}