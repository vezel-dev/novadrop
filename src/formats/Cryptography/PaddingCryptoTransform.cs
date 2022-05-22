namespace Vezel.Novadrop.Cryptography;

public sealed class PaddingCryptoTransform : ICryptoTransform
{
    public int InputBlockSize => _transform.InputBlockSize;

    public int OutputBlockSize => _transform.OutputBlockSize;

    public bool CanTransformMultipleBlocks => _transform.CanTransformMultipleBlocks;

    public bool CanReuseTransform => _transform.CanReuseTransform;

    readonly ICryptoTransform _transform;

    public PaddingCryptoTransform(ICryptoTransform transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        _transform = transform;
    }

    ~PaddingCryptoTransform()
    {
        Dispose();
    }

    public void Dispose()
    {
        _transform.Dispose();
        GC.SuppressFinalize(this);
    }

    public int TransformBlock(
        byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
        return _transform.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        ArgumentNullException.ThrowIfNull(inputBuffer);

        var input = new byte[InputBlockSize];

        inputBuffer.AsSpan(inputOffset, inputCount).CopyTo(input);

        return _transform.TransformFinalBlock(input, 0, input.Length);
    }
}
