using System;
using System.Collections.Generic;

namespace Code.Game.StaticData.Configs
{
    public static class ResearchCatalogMigration
    {
        public static ResearchBranchDefinition[] FromFlat(ResearchNodeDefinition[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                return Array.Empty<ResearchBranchDefinition>();

            var count = nodes.Length;
            var indexById = new Dictionary<ResearchId, int>(count);
            var childIndexes = new List<int>[count];

            for (var i = 0; i < count; i++)
            {
                childIndexes[i] = new List<int>(4);
                var node = nodes[i];

                if (node == null || node.Id == ResearchId.None)
                    continue;

                indexById[node.Id] = i;
            }

            var roots = new List<int>(8);

            for (var i = 0; i < count; i++)
            {
                var node = nodes[i];

                if (node == null)
                    continue;

                if (node.Parent != ResearchId.None
                    && indexById.TryGetValue(node.Parent, out var parentIndex)
                    && parentIndex != i)
                {
                    childIndexes[parentIndex].Add(i);
                    continue;
                }

                roots.Add(i);
            }

            var visited = new bool[count];
            var branches = new List<ResearchBranchDefinition>(roots.Count);

            for (var i = 0; i < roots.Count; i++)
            {
                var branch = Build(nodes, childIndexes, visited, roots[i]);

                if (branch != null)
                    branches.Add(branch);
            }

            for (var i = 0; i < count; i++)
            {
                if (nodes[i] == null || visited[i])
                    continue;

                var branch = Build(nodes, childIndexes, visited, i);

                if (branch != null)
                    branches.Add(branch);
            }

            return branches.ToArray();
        }

        private static ResearchBranchDefinition Build(
            ResearchNodeDefinition[] nodes,
            List<int>[] childIndexes,
            bool[] visited,
            int start)
        {
            if (start < 0 || visited[start] || nodes[start] == null)
                return null;

            var direction = nodes[start].Direction;
            var entries = new List<ResearchBranchEntry>(8);
            var current = start;

            while (current >= 0 && !visited[current])
            {
                visited[current] = true;
                entries.Add(nodes[current]);

                var children = childIndexes[current];
                var continuation = -1;

                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    if (visited[child])
                        continue;

                    if (continuation < 0 && nodes[child].Direction == direction)
                    {
                        continuation = child;
                        continue;
                    }

                    var nested = Build(nodes, childIndexes, visited, child);

                    if (nested != null)
                        entries.Add(nested);
                }

                current = continuation;
            }

            return new ResearchBranchDefinition(BranchName(nodes[start].Id), direction, entries.ToArray());
        }

        private static string BranchName(ResearchId id)
        {
            var name = id.ToString();
            var end = name.Length;

            while (end > 0 && char.IsDigit(name[end - 1]))
                end--;

            if (end == 0)
                return name;

            return name.Substring(0, end);
        }
    }
}
