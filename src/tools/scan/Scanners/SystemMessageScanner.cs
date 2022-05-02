using static Iced.Intel.AssemblerRegisters;

namespace Vezel.Novadrop.Scanners;

sealed class SystemMessageScanner : IScanner
{
    struct ThreadArgs
    {
        public uint Count;

        public nuint Results;
    }

    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x85, 0xc9,                         // test ecx, eax
        0x78, 0x17,                         // js short 0x17
        0x81, 0xf9, null, null, null, null, // cmp ecx, <count>
        0x73, 0x0f,                         // jae short 0xf
    };

    public unsafe void Run(ScanContext context)
    {
        var process = context.Process;
        var module = process.MainModule;

        Console.WriteLine("Searching for system message name function...");

        var o = module.Search(_pattern).Cast<nuint?>().FirstOrDefault();

        if (o is not nuint off)
            throw new ApplicationException("Could not find system message name function.");

        var count = module.Read<uint>(off + 6);

        if (count < 4000)
            throw new ApplicationException("Could not read system message count.");

        var results = process.Alloc((nuint)sizeof(nuint) * count, MemoryFlags.Read | MemoryFlags.Write);

        try
        {
            var args = process.Alloc((nuint)sizeof(ThreadArgs), MemoryFlags.Read | MemoryFlags.Write);

            try
            {
                new MemoryWindow(process, args, (nuint)sizeof(ThreadArgs)).Write(0, new ThreadArgs
                {
                    Count = count,
                    Results = results,
                });

                using var code = DynamicCode.Create(process, asm =>
                {
                    var loop = asm.CreateLabel();

                    asm.push(r12);
                    asm.push(r13);
                    asm.push(r14);
                    asm.push(r15);
                    asm.push(rdi);

                    asm.mov(r12, rcx);
                    asm.mov(r13, __dword_ptr[r12 + (long)Marshal.OffsetOf<ThreadArgs>(nameof(ThreadArgs.Count))]);
                    asm.mov(r14, __qword_ptr[r12 + (long)Marshal.OffsetOf<ThreadArgs>(nameof(ThreadArgs.Results))]);
                    asm.mov(r15, module.ToAddress(off));
                    asm.mov(rdi, 0);

                    asm.Label(ref loop);

                    asm.mov(rcx, rdi);
                    asm.call(r15);
                    asm.mov(__qword_ptr[r14 + rdi * sizeof(nuint)], rax);

                    asm.inc(rdi);
                    asm.cmp(rdi, r13);
                    asm.jb(loop);

                    asm.pop(rdi);
                    asm.pop(r12);
                    asm.pop(r13);
                    asm.pop(r14);
                    asm.pop(r15);

                    asm.mov(rax, 42);
                    asm.ret();
                });

                var ret = code.Call(args);

                process.Free(args);

                if (ret != 42)
                    throw new ApplicationException($"Could not remotely retrieve system message table ({ret}).");

                var messages = new Dictionary<string, uint>((int)count);
                var resultsWindow = new MemoryWindow(process, results, (nuint)sizeof(nuint) * count);

                for (var i = 0u; i < count; i++)
                {
                    if (!module.TryGetOffset(resultsWindow.Read<nuint>((nuint)sizeof(nuint) * i), out var offset))
                        throw new ApplicationException("Could not read system message strings.");

                    var sb = new StringBuilder(128);
                    var j = 0u;

                    while (true)
                    {
                        var ch = module.Read<char>(offset + sizeof(char) * j);

                        if (ch == '\0')
                            break;

                        _ = sb.Append(ch);

                        j++;
                    }

                    messages.Add(sb.ToString(), i);
                }

                Console.WriteLine($"Found system messages: {count}");

                File.WriteAllLines(
                    Path.Combine(context.Output.FullName, "SystemMessages.txt"),
                    messages.OrderBy(x => x.Key).Select(x => $"[\"{x.Key}\"] = {x.Value},"));
            }
            finally
            {
                process.Free(args);
            }
        }
        finally
        {
            process.Free(results);
        }
    }
}
