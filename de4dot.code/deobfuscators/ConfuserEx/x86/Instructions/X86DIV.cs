using System.Collections.Generic;
using Iced.Intel;

namespace de4dot.code.deobfuscators.ConfuserEx.x86.Instructions
{
    class X86DIV : X86Instruction
    {
        public X86DIV(in Instruction rawInstruction)
        {
            // div r/m32 has one explicit operand (EDX:EAX is implicit). Execute() is a no-op, as before.
            Operands = new IX86Operand[1];
            Operands[0] = GetOperand(rawInstruction, 0);
        }

        public override X86OpCode OpCode { get { return X86OpCode.DIV; } }

        public override void Execute(Dictionary<string, int> registers, Stack<int> localStack)
        {
            
        }
    }
}
