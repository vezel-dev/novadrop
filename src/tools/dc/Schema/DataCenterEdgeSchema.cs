namespace Vezel.Novadrop.Schema;

internal sealed class DataCenterEdgeSchema
{
    public DataCenterNodeSchema Node { get; }

    public bool IsOptional { get; set; }

    public bool IsRepeatable { get; set; }

    public DataCenterEdgeSchema(DataCenterNodeSchema node)
    {
        Node = node;
    }
}
