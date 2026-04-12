using System.Linq;
using Code.Game.Common.Entity.ToStrings;
using Entitas;

// ReSharper disable once CheckNamespace
public sealed partial class InputEntity : INamedEntity
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
        return components.FirstOrDefault().GetType().Name;
    }

    public string BaseToString() => base.ToString();
}
