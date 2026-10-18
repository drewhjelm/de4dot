using System.Collections.Generic;
using Iced.Intel;

namespace de4dot.code.deobfuscators.ConfuserEx.x86.Instructions
{
    class X86NEG : X86Instruction
    {
        public X86NEG(in Instruction rawInstruction)
        {
            Operands = new IX86Operand[1];
            Operands[0] = GetOperand(rawInstruction, 0);
        }

        public override X86OpCode OpCode { get { return X86OpCode.NEG; } }

        public override void Execute(Dictionary<string, int> registers, Stack<int> localStack)
        {
            registers[((X86RegisterOperand) Operands[0]).Register.ToString()] =
                -registers[((X86RegisterOperand) Operands[0]).Register.ToString()];
        }
    }
}
