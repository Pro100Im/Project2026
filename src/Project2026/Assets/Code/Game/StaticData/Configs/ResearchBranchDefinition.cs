using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [Serializable]
    public abstract class ResearchBranchEntry
    {
    }

    [Serializable]
    public class ResearchBranchDefinition : ResearchBranchEntry
    {
        [field: SerializeField] public string Name { get; private set; } = "Branch";
        [field: SerializeField] public ResearchLinkDirection Direction { get; private set; }
        [SerializeReference] public ResearchBranchEntry[] Entries = Array.Empty<ResearchBranchEntry>();

        public ResearchBranchDefinition()
        {
        }

        public ResearchBranchDefinition(string name, ResearchLinkDirection direction, ResearchBranchEntry[] entries)
        {
            Name = string.IsNullOrEmpty(name) ? "Branch" : name;
            Direction = direction;
            Entries = entries ?? Array.Empty<ResearchBranchEntry>();
        }
    }
}
