using System;
using System.Collections.Generic;
using System.Linq;
using Iced.Intel;
using de4dot.code.deobfuscators.ConfuserEx.x86.Instructions;
using dnlib.DotNet;

namespace de4dot.code.deobfuscators.ConfuserEx.x86
{
    public sealed class X86Method
    {
        public List<X86Instruction> Instructions;

        public Stack<int> LocalStack = new Stack<int>();
        public Dictionary<string, int> Registers = new Dictionary<string, int>
        {
            {"EAX", 0},
            {"EBX", 0},
            {"ECX", 0},
            {"EDX", 0},
            {"ESP", 0},
            {"EBP", 0},
            {"ESI", 0},
            {"EDI", 0}
        };

        private readonly ModuleDefMD _module;
        public X86Method(MethodDef method,ModuleDefMD module)
        {
            this._module = module;
            Instructions = new List<X86Instruction>();
            ParseInstructions(ReadCode(method, module));
        }

        public X86Method(byte[] code)
        {
            Instructions = new List<X86Instruction>();
            ParseInstructions(code);
        }

        // Bounds for input-controlled native code: bytes read and instructions decoded.
        private const int MaxCodeBytes = 4096;
        private const int MaxInstructions = 1024;

        private void ParseInstructions(byte[] code)
        {
            var rawInstructions = new List<Instruction>();
            var decoder = Decoder.Create(32, new ByteArrayCodeReader(code));

            while (rawInstructions.Count < MaxInstructions)
            {
                decoder.Decode(out var instr);
                if (instr.IsInvalid || instr.Length == 0)
                    break;
                if (instr.Mnemonic == Mnemonic.Ret) //TODO: Check if this is the only return in function, e.g. check for jumps that go beyond this address
                    break;
                rawInstructions.Add(instr);
                if (decoder.IP >= (ulong)code.Length)
                    break;
            }

            while (rawInstructions.Count > 0 && rawInstructions[rawInstructions.Count - 1].Mnemonic == Mnemonic.Pop)
                rawInstructions.RemoveAt(rawInstructions.Count - 1);

            foreach (var instr in rawInstructions)
            {
                switch (instr.Mnemonic)
                {
                    case Mnemonic.Mov:
                        Instructions.Add(new X86MOV(instr));
                        break;
                    case Mnemonic.Add:
                        Instructions.Add(new X86ADD(instr));
                        break;
                    case Mnemonic.Sub:
                        Instructions.Add(new X86SUB(instr));
                        break;
                    case Mnemonic.Imul:
                        Instructions.Add(new X86IMUL(instr));
                        break;
                    case Mnemonic.Div:
                        Instructions.Add(new X86DIV(instr));
                        break;
                    case Mnemonic.Neg:
                        Instructions.Add(new X86NEG(instr));
                        break;
                    case Mnemonic.Not:
                        Instructions.Add(new X86NOT(instr));
                        break;
                    case Mnemonic.Xor:
                        Instructions.Add(new X86XOR(instr));
                        break;
                    case Mnemonic.Pop:
                        Instructions.Add(new X86POP(instr));
                        break;
                    default:
	                    Logger.w("ConfuserEx native: Unhandled instruction {0}", instr.ToString());
	                    break;
                }
            }
        }

        private static byte[] ReadCode(MethodDef method, ModuleDefMD module)
        {
            var reader = module.Metadata.PEImage.CreateReader();
            //TODO: Don't use hardcoded offset
            long start = (long)(uint)module.Metadata.PEImage.ToFileOffset(method.RVA) + 20; // skip to actual calculation code
            if (start < 0 || start >= reader.Length)
                return Array.Empty<byte>();
            int count = (int)Math.Min(MaxCodeBytes, reader.Length - start);
            reader.Position = (uint)start;
            return reader.ReadBytes(count);
        }

        public int Execute(params int[] @params)
        {
            foreach (var param in @params)
                LocalStack.Push(param);

            foreach (var instr in Instructions)
                instr.Execute(Registers, LocalStack);

            return Registers["EAX"];
        }
    }
}
