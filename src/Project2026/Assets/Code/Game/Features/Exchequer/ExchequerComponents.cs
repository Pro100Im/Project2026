using Entitas;

namespace Code.Game.Features.Exchequer
{
    [Game, Meta] public struct GameExchequer : IComponent { public GameExchequerView Value; };
    [Game, Meta] public struct ExchequerGoldChangeRequest : IComponent { public int Value; };
    [Game, Meta] public struct ExchequerMealChangeRequest : IComponent { public int Value; };
    [Game, Meta] public struct ExchequerManaChangeRequest : IComponent { public int Value; };

    [Game, Meta] public struct ExchequerGoldCapacity : IComponent { public int Value; }
    [Game, Meta] public struct ExchequerMealCapacity : IComponent { public int Value; }
    [Game, Meta] public struct ExchequerManaCapacity : IComponent { public int Value; }
}