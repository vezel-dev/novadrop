namespace Vezel.Novadrop.Schema;

internal static class DataCenterSchemaInference
{
    public static DataCenterNodeSchema Infer(
        DataCenterSchemaStrategy strategy, DataCenterNode root, Action increment, CancellationToken cancellationToken)
    {
        var nodeSchema = new DataCenterNodeSchema();

        InferSchema(strategy, root, nodeSchema, false, increment, cancellationToken);

        return nodeSchema;
    }

    private static void InferSchema(
        DataCenterSchemaStrategy strategy,
        DataCenterNode node,
        DataCenterNodeSchema nodeSchema,
        bool nodeExistedBefore,
        Action? increment,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (node.Keys is { HasAttributeNames: true } keys && !nodeSchema.HasKeys)
            nodeSchema.SetKeys(keys.AttributeNames);

        // Some ~225 nodes in official data center files have __value__ set even when they have children, but the
        // strings are random symbols or broken XML tags. The fact that they are included is most likely a bug in the
        // software used to pack those files. So, just ignore the value in these cases.
        if (node.Value != null && node.Children.Count == 0)
            nodeSchema.HasValue = true;

        nodeSchema.UpdateAttributeBounds(
            nodeExistedBefore, node.Attributes.ToDictionary(item => item.Key, item => item.Value.TypeCode));

        var childCounts = new Dictionary<string, int>();

        foreach (var group in node.Children.GroupBy(child => child.Name))
        {
            childCounts[group.Key] = group.Count();

            var edgeSchema = nodeSchema.GetEdge(
                strategy, node.Name, group.Key, out var edgeExistedBefore, out var childSchemaExistedBefore);

            if (!edgeExistedBefore)
                edgeSchema.IsOptional = nodeExistedBefore;

            var firstInGroup = true;

            foreach (var child in group)
            {
                InferSchema(
                    strategy,
                    child,
                    edgeSchema.Node,
                    childSchemaExistedBefore || !firstInGroup,
                    null,
                    cancellationToken);

                firstInGroup = false;

                increment?.Invoke();
            }
        }

        nodeSchema.UpdateEdgeBounds(childCounts);
    }
}
