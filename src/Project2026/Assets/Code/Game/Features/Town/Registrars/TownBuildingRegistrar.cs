using Code.Game.Features.Town.Views;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Town.Registrars
{
    public class TownBuildingRegistrar : TownEntityComponentRegistrar
    {
        [SerializeField] private TownBuildingConfig _config;
        [SerializeField] private TownView _view;

        public override void RegisterComponents()
        {
            Entity.isTownBuilding = true;
            Entity.AddTownBuildingType(_config.Type);
            Entity.AddTownBuildingLevel(0);
            Entity.AddTownBuildingMaxLevel(_config.Levels.Length);
            Entity.AddTownBuildingConfig(_config);
            Entity.AddTownView(_view);
        }

        public override void UnregisterComponents()
        {
            Entity.isTownBuilding = false;

            if (Entity.hasTownBuildingType)
                Entity.RemoveTownBuildingType();
            if (Entity.hasTownBuildingLevel)
                Entity.RemoveTownBuildingLevel();
            if (Entity.hasTownBuildingMaxLevel)
                Entity.RemoveTownBuildingMaxLevel();
            if (Entity.hasTownBuildingConfig)
                Entity.RemoveTownBuildingConfig();
            if (Entity.hasTownView)
                Entity.RemoveTownView();
        }
    }
}
