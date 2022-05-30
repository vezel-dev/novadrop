namespace Vezel.Novadrop.Data.Serialization;

static class DataCenterNameTree
{
    sealed class DataCenterNameNode
    {
        public string Name { get; }

        public Dictionary<string, bool> Attributes { get; } = new();

        public Dictionary<string, DataCenterNameNode> Children { get; } = new();

        public DataCenterNameNode(string name)
        {
            Name = name;
        }
    }

    public static void Collect(DataCenterNode root, Action<string> handler, CancellationToken cancellationToken)
    {
        var nameRoot = new DataCenterNameNode(root.Name);

        void ConstructTree(DataCenterNode dataNode, DataCenterNameNode nameNode)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var keys = dataNode.Keys;

            // There can be keys that refer to attributes that do not exist even in the official data center, so we need
            // to explicitly add these attribute names.
            if (keys.HasAttributeNames)
                foreach (var name in keys.AttributeNames)
                    nameNode.Attributes[name] = true;

            if (dataNode.HasAttributes)
            {
                foreach (var (name, _) in dataNode.Attributes)
                {
                    ref var isKey = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        nameNode.Attributes, name, out var exists);

                    if (!exists)
                        isKey |= false;
                }
            }

            if (dataNode.HasChildren)
            {
                foreach (var dataChild in dataNode.Children)
                {
                    ref var nameChild = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        nameNode.Children, dataChild.Name, out var exists);

                    if (!exists)
                        nameChild = new(dataChild.Name);

                    ConstructTree(dataChild, nameChild!);
                }
            }
        }

        ConstructTree(root, nameRoot);

        void InvokeHandler(string name)
        {
            if (name is not (DataCenterConstants.RootNodeName or DataCenterConstants.ValueAttributeName))
                handler(name);
        }

        void WalkTree(DataCenterNameNode nameNode)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InvokeHandler(nameNode.Name);

            var attrs = nameNode.Attributes;
            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach (var (name, _) in attrs.Where(kvp => kvp.Value).OrderBy(kvp => kvp.Key, comparer))
                InvokeHandler(name);

            foreach (var (name, _) in attrs.Where(kvp => !kvp.Value).OrderBy(kvp => kvp.Key, comparer))
                InvokeHandler(name);

            foreach (var (_, child) in nameNode.Children.OrderBy(kvp => kvp.Key, comparer))
                WalkTree(child);
        }

        WalkTree(nameRoot);

        // These must always go last and must always be present.
        handler(DataCenterConstants.RootNodeName);
        handler(DataCenterConstants.ValueAttributeName);
    }
}
