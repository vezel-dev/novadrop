namespace Vezel.Novadrop.Cryptography;

[SuppressMessage("", "CA5358")]
public sealed class Xor256 : SymmetricAlgorithm
{
    private sealed class Xor256CryptoTransform : ICryptoTransform
    {
        public bool CanReuseTransform => true;

        public bool CanTransformMultipleBlocks => true;

        public int InputBlockSize => _key.Length;

        public int OutputBlockSize => _key.Length;

        private readonly ReadOnlyMemory<byte> _key;

        public Xor256CryptoTransform(byte[] key)
        {
            _key = key.ToArray();
        }

        public void Dispose()
        {
            // Nothing to clean up.
        }

        public bool Transform(scoped ReadOnlySpan<byte> source, scoped Span<byte> destination, out int written)
        {
            if (destination.Length < source.Length)
            {
                written = 0;

                return false;
            }

            for (var i = 0; i < source.Length; i++)
                Unsafe.Add(ref MemoryMarshal.GetReference(destination), i) =
                    (byte)(Unsafe.Add(ref MemoryMarshal.GetReference(source), i) ^
                    Unsafe.Add(ref MemoryMarshal.GetReference(_key.Span), i % KeyLength));

            written = source.Length;

            return true;
        }

        public int TransformBlock(
            byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Check.Null(inputBuffer);
            Check.Range(inputOffset >= 0 && inputOffset <= inputBuffer.Length, inputOffset);
            Check.Range(inputCount >= 0 && inputCount <= inputBuffer.Length - inputOffset, inputCount);
            Check.Null(outputBuffer);
            Check.Range(
                outputOffset <= outputBuffer.Length && inputCount <= outputBuffer.Length - outputOffset, outputOffset);

            _ = Transform(
                inputBuffer.AsSpan(inputOffset, inputCount), outputBuffer.AsSpan(outputOffset), out var written);

            return written;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            Check.Null(inputBuffer);
            Check.Range(inputOffset >= 0 && inputOffset <= inputBuffer.Length, inputOffset);
            Check.Range(inputCount >= 0 && inputCount <= inputBuffer.Length - inputOffset, inputCount);

            var arr = GC.AllocateUninitializedArray<byte>(inputCount);

            // Nothing special to be done for the last block, but we do need to return a new array.
            _ = TransformBlock(inputBuffer, inputOffset, inputCount, arr, 0);

            return arr;
        }
    }

    private const int KeyLength = 32;

    public override int FeedbackSize
    {
        get => base.FeedbackSize;
        set
        {
            Check.Crypto(value == 0);

            FeedbackSizeValue = value;
        }
    }

    public override CipherMode Mode
    {
        get => base.Mode;
        set
        {
            Check.Crypto(value == CipherMode.ECB);

            ModeValue = value;
        }
    }

    public override PaddingMode Padding
    {
        get => base.Padding;
        set
        {
            // TODO: Support padding?
            Check.Crypto(value == PaddingMode.None);

            PaddingValue = value;
        }
    }

    private Xor256()
    {
        ModeValue = CipherMode.ECB;
        PaddingValue = PaddingMode.None;
        KeySizeValue = KeyLength * 8;
        BlockSizeValue = KeySizeValue;
        LegalKeySizesValue = new[]
        {
            new KeySizes(KeySizeValue, KeySizeValue, 0),
        };
        LegalBlockSizesValue = new[]
        {
            new KeySizes(BlockSizeValue, BlockSizeValue, 0),
        };
    }

    public static new Xor256 Create()
    {
        return new();
    }

    public override void GenerateIV()
    {
        // The IV is not actually used, but the SymmetricAlgorithm contract requires us to generate one anyway.
        IVValue = RandomNumberGenerator.GetBytes(KeyLength);
    }

    public override void GenerateKey()
    {
        KeyValue = RandomNumberGenerator.GetBytes(KeyLength);
    }

    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV)
    {
        return new Xor256CryptoTransform(Key);
    }

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV)
    {
        return new Xor256CryptoTransform(Key);
    }

    protected override bool TryDecryptEcbCore(
        ReadOnlySpan<byte> ciphertext, Span<byte> destination, PaddingMode paddingMode, out int bytesWritten)
    {
        Check.Range(paddingMode == PaddingMode.None, paddingMode);

        using var decryptor = Unsafe.As<Xor256CryptoTransform>(CreateDecryptor());

        return decryptor.Transform(ciphertext, destination, out bytesWritten);
    }

    protected override bool TryEncryptEcbCore(
        ReadOnlySpan<byte> plaintext, Span<byte> destination, PaddingMode paddingMode, out int bytesWritten)
    {
        Check.Range(paddingMode == PaddingMode.None, paddingMode);

        using var encryptor = Unsafe.As<Xor256CryptoTransform>(CreateEncryptor());

        return encryptor.Transform(plaintext, destination, out bytesWritten);
    }
}
