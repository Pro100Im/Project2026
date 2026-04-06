using Code.Infrastructure.View.Registrars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common.Registrars
{
    public class UIDocRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private UIDocument _doc;

        public override void RegisterComponents()
        {
            Entity.AddUIDocument(_doc);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasUIDocument)
                Entity.RemoveUIDocument();
        }
    }
}