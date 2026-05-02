using Code.Game.Common.Entity.ToStrings;
using Code.Game.Features.Health;
using Code.Game.Features.Player;
using Code.Game.Features.Tower;
using Code.Game.Features.Unit;
using Entitas;
using System;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed partial class GameEntity : INamedEntity
{
    private EntityPrinter _printer;

    public override string ToString()
    {
        if (_printer == null)
            _printer = new EntityPrinter(this);

        _printer.InvalidateCache();

        return _printer.BuildToString();
    }

    public string EntityName(IComponent[] components)
    {
        try
        {
            if (components.Length == 1)
                return components[0].GetType().Name;

            foreach (IComponent component in components)
            {
                var entityName = component.GetType().Name;

                switch (component.GetType().Name)
                {
                    case nameof(Unit):
                        return $"Unit";
                    case nameof(Tower):
                        return $"Tower";
                    case nameof(TowerPlace):
                        return $"TowerPlace";
                    case nameof(HpBar):
                        return $"HpBar";
                    case nameof(PlayerCastle):
                        return $"PlayerCastle";
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
        }

        return components.First().GetType().Name;
    }

    public string BaseToString() => base.ToString();
}

