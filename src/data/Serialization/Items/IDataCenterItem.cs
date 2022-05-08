namespace Vezel.Novadrop.Data.Serialization.Items;

interface IDataCenterItem<T>
    where T : unmanaged
{
    static abstract void ReverseEndianness(ref T item);
}
