// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawKeys : IDataCenterItem
{
    public ushort NameIndex1;

    public ushort NameIndex2;

    public ushort NameIndex3;

    public ushort NameIndex4;

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawKeys);
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        NameIndex1 = reader.ReadUInt16();
        NameIndex2 = reader.ReadUInt16();
        NameIndex3 = reader.ReadUInt16();
        NameIndex4 = reader.ReadUInt16();
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteUInt16(NameIndex1);
        writer.WriteUInt16(NameIndex2);
        writer.WriteUInt16(NameIndex3);
        writer.WriteUInt16(NameIndex4);
    }
}
