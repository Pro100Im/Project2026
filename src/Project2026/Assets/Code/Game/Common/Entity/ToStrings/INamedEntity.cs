using Entitas;

namespace Code.Game.Common.Entity.ToStrings
{
  public interface INamedEntity : IEntity
  {
    string EntityName(IComponent[] components);
    string BaseToString();
  }
}