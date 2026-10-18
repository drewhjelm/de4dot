using System.Collections.Generic;
using Iced.Intel;

namespace de4dot.code.deobfuscators.ConfuserEx.x86.Instructions
{
    internal class X86MOV : X86Instruction
    {
        public X86MOV(in Instruction rawInstruction)
        {
            Operands = new IX86Operand[2];
            Operands[0] = GetOperand(rawInstruction, 0);
            Operands[1] = GetOperand(rawInstruction, 1);
        }

        public override X86OpCode OpCode
        {
            get { return X86OpCode.MOV; }
        }

        public override void Execute(Dictionary<string, int> registers, Stack<int> localStack)
        {
            if (Operands[1] is X86ImmediateOperand)
                registers[((X86RegisterOperand) Operands[0]).Register.ToString()] =
                    (Operands[1] as X86ImmediateOperand).Immediate;
            else
            {
                var regOperand = (X86RegisterOperand) Operands[0];
                registers[regOperand.Register.ToString()] =
                   registers[(Operands[1] as X86RegisterOperand).Register.ToString()];
            }
        }
    }
}
