using System;
using System.Linq;
using Code.Common.Entity.ToStrings;
using Entitas;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed partial class MetaEntity : INamedEntity
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
        return components.First().GetType().Name;
    }

    public string BaseToString() => base.ToString();
}