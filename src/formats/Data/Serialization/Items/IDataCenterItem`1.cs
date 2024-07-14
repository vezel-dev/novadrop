// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Data.Serialization.Items;

internal interface IDataCenterItem<T> : IEqualityOperators<T, T, bool>, IEquatable<T>
    where T : struct, IDataCenterItem<T>
{
    public static abstract int GetSize(DataCenterArchitecture architecture);

    public static abstract void Read(ref SpanReader reader, DataCenterArchitecture architecture, out T item);

    public static abstract void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in T item);

    public abstract string ToString();
}
