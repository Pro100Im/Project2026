using Entitas;

namespace Code.Game.Features.Exchequer
{
    [Game, Meta] public struct Exchequer : IComponent { };

    [Game, Meta] public struct ExchequerCapacity : IComponent { public int Value; }

    [Game] public struct ExchequerTypeComponent : IComponent { public GameExchequerType Value; }

    public enum GameExchequerType
    {
        Gold,
        Meal,
        Mana
    } 
}