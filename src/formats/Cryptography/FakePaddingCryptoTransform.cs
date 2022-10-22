namespace Vezel.Novadrop.Cryptography;

public sealed class FakePaddingCryptoTransform : ICryptoTransform
{
    public int InputBlockSize => _transform.InputBlockSize;

    public int OutputBlockSize => _transform.OutputBlockSize;

    public bool CanTransformMultipleBlocks => _transform.CanTransformMultipleBlocks;

    public bool CanReuseTransform => _transform.CanReuseTransform;

    private readonly ICryptoTransform _transform;

    public FakePaddingCryptoTransform(ICryptoTransform transform)
    {
        Check.Null(transform);

        _transform = transform;
    }

    ~FakePaddingCryptoTransform()
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
        Check.Null(inputBuffer);

        // The issue we are solving here is that various algorithms (e.g. AES) only want to operate on a final block
        // whose length is a multiple of the block size. So we create an array that satisfies that condition, copy the
        // data we want to decrypt into it (leaving the rest zeroed), decrypt the block-sized array, and finally return
        // only the 'real' decrypted bytes.

        var input = new byte[InputBlockSize];

        inputBuffer.AsSpan(inputOffset, inputCount).CopyTo(input);

        var result = _transform.TransformFinalBlock(input, 0, input.Length);

        Array.Resize(ref result, inputCount);

        return result;
    }
}
