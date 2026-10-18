using System;
using System.Collections.Generic;
using System.Globalization;
using Iced.Intel;

namespace de4dot.code.deobfuscators.ConfuserEx.x86
{
	public enum X86OpCode
	{
		MOV,
		ADD,
		SUB,
		IMUL,
		DIV,
		NEG,
		NOT,
		XOR,
		POP,
		PUSH
	}

	public enum X86Register
	{
		EAX,
		ECX,
		EDX,
		EBX,
		ESP,
		EBP,
		ESI,
		EDI
	}

	public interface IX86Operand
	{
	}

	public class X86RegisterOperand : IX86Operand
	{
		public X86Register Register { get; set; }

		public X86RegisterOperand(X86Register reg)
		{
			Register = reg;
		}
	}

	public class X86ImmediateOperand : IX86Operand
	{
		public int Immediate { get; set; }

		public X86ImmediateOperand(int imm)
		{
			Immediate = imm;
		}
	}

	public abstract class X86Instruction
	{
		public abstract X86OpCode OpCode { get; }
		public IX86Operand[] Operands { get; set; }
		public abstract void Execute(Dictionary<string, int> registers, Stack<int> localStack);

		// Operands come from the managed Iced decoder (no native disassembler). Only 32-bit
		// general purpose registers and immediates are accepted, as before.
		public static IX86Operand GetOperand(in Instruction instruction, int operand)
		{
			if (operand >= instruction.OpCount)
				throw new Exception("Missing operand");
			switch (instruction.GetOpKind(operand))
			{
				case OpKind.Register:
					var reg = instruction.GetOpRegister(operand);
					if (reg < Register.EAX || reg > Register.EDI)
						throw new Exception("Unexpected register: " + reg);
					return new X86RegisterOperand((X86Register)(reg - Register.EAX));
				case OpKind.Immediate8:
				case OpKind.Immediate16:
				case OpKind.Immediate32:
				case OpKind.Immediate8to16:
				case OpKind.Immediate8to32:
					return new X86ImmediateOperand((int)instruction.GetImmediate(operand));
				default:
					throw new Exception("Unexpected operand type: " + instruction.GetOpKind(operand));
			}
		}
	}
}
