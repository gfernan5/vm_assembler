/* 
 * File: Instructions.cs
 * Authors: David Long, Ethan Miller, Gian Fernandez, Lexy Andershock
 * Date: 2025-04-04
 *
 * Brief: Define classes for each instruction.
 */

// Exit Instruction
public class Exit : IInstruction {
    private readonly int _exitCode;
    public Exit(int exitCode = 0) {
        _exitCode = exitCode & 0x0000_00FF;
    }
    public int Encode() {
        return (0b0000 << 24) | _exitCode;
    }
}

// Swap Instruction
public class Swap : IInstruction {
    private readonly int _from;
    private readonly int _to;
    public Swap(int from = 4, int to = 0) {
        _from = from / 4;
        _to = to / 4;
    }
    public int Encode() {
        int fromEncoded = (_from & 0x0FFF) << 12;
        int toEncoded = (_to & 0x0FFF);
        return (0b0001 << 24) | fromEncoded | toEncoded;
    }
}

// NOP Instruction
public class Nop : IInstruction {
    public int Encode() {
        return (0b0010 << 24);
    }
}

// Input Instruction
public class Input : IInstruction {
    public int Encode() {
        return (0b0100 << 24);
    }
}

// String Input Instruction
public class Stinput : IInstruction {
    private readonly int _unsignedChars;
    public Stinput(int unsignedChars) {
        if (unsignedChars < 0 || unsignedChars > 0x00FF_FFFF) {
            unsignedChars = 0x00FF_FFFF;
        }
        _unsignedChars = unsignedChars;
    }
    public int Encode() {
        return (0b0101 << 24) | _unsignedChars;
    }
}

// Dup Instruction
public class Dup : IInstruction {
    private readonly int _offset;
    public Dup(int offset) {
        _offset = offset & ~3;
    }
    public int Encode() {
        return (0b1100 << 28) | _offset;
    }
}

// Dump Instruction
public class Dump : IInstruction {
    public int Encode() {
        return (0b1110 << 28);
    }
}

/* Binary Arithmetic Instructions */

// add instruction
public class Add : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0000 << 24;
        return opcode | operation;
    }
}

// sub instruction
public class Sub : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0001 << 24;
        return opcode | operation;
    }
}

// mul instruction
public class Mul : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0010 << 24;
        return opcode | operation;
    }
}

// div instruction
public class Div : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0011 << 24;
        return opcode | operation;
    }
}

// rem instruction
public class Rem : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0100 << 24;
        return opcode | operation;
    }
}

// and instruction
public class And : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0101 << 24;
        return opcode | operation;
    }
}

// or instruction
public class Or : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0110 << 24;
        return opcode | operation;
    }
}

// xor instruction
public class Xor : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b0111 << 24;
        return opcode | operation;
    }
}

// lsl instruction
public class Lsl : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b1000 << 24;
        return opcode | operation;
    }
}

// lsr instruction
public class Lsr : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b1001 << 24;
        return opcode | operation;
    }
}

// asr instruction
public class Asr : IInstruction
{
    public int Encode()
    {
        int opcode = 0b0010 << 28;
        int operation = 0b1011 << 24;
        return opcode | operation;
    }
}

// Debug
public class Debug : IInstruction {
    private readonly int _value;

    public Debug(int value = 0) {
        _value = value & 0x00FF_FFFF;
    }

    public int Encode() {
        return (0b0000 << 28) | (0b1111 << 24) | _value;
    }
}

// Pop - Opcode 1
public class Pop : IInstruction {
    private readonly int _offset;

    public Pop(int offset = 4) {
        _offset = offset >> 2;
    }

    public int Encode() {
        return (0b0001 << 28) | (_offset << 2);
    }
}

// Neg - Opcode 3
public class Neg : IInstruction {
    public int Encode() {
        return (0b0011 << 28) | (0b0000 << 24);
    }
}

// Not - Opcode 3
public class Not : IInstruction {
    public int Encode() {
        return (0b0011 << 28) | (0b0001 << 24);
    }
}

// String print - Opcode 4
public class Stprint : IInstruction {
    private readonly int _offset;

    public Stprint(int offset = 0) {
        _offset = offset;
    }

    public int Encode() {
        return (0b0100 << 28) | (_offset & 0x0FFFFFFF);
    }
}

// Print - Opcode 13
public class Print : IInstruction {
    private readonly int _fmt;
    private readonly int _offset;

    public Print(int fmt = 0, int offset = 0) {
        _offset = offset & 0x0FFFFFFC;
        if(fmt == 1) {
            _fmt = 0b0001;
        }
        else if(fmt == 2) {
            _fmt = 0b0010;
        }
        else if(fmt == 3) {
            _fmt = 0b0011;
        }
        else {
            _fmt = 0b0000;
        }
    }

    public int Encode() {
        return (0b1101 << 28) | _offset | _fmt;
    }
}

// Unconditional Goto
// Extract PC from label and put into PC offset of instruction
public class Goto : IInstruction {
    private readonly int _label;
    public Goto(int label) {
        _label = label;
    }
    public int Encode() {
        return (0b0111 << 28) | _label;
    }
}

// Call
// Extract PC, clear out bottom bits, place into offset of instruction
public class Call : IInstruction {
    private readonly int _label;
    public Call(int label) {
        _label = label & ~3;
    }
    public int Encode() {
        return (0b0101 << 28) | _label;
    }
}

// Return (OPTIONAL: deal with this in main!)
// takes an offset, zeros out bottom bits, places it into the stack offset
public class Return : IInstruction {
    private readonly int _label;
    public Return(int label) {
        _label = label & ~3;
    }
    public int Encode() {
        return (0b0110 << 28) | _label;
    }
}

// Unary If
// This will require translation in main! Expecting ints instead of strings.
// Expects a 1001, 0, condition, PC relative
public class UnaryIf : IInstruction {
    private readonly int _cond;
    private readonly int _label;
    public UnaryIf(int cond, int label) {
        _cond = cond;
        _label = label;
    }
    public int Encode() {
        return (0b1001 << 28) | (0b0 << 27) | (_cond << 25) | _label;
    }
}

// Binary If
// if <cond> <label>
// Takes 0b1000, condition, PC relative offset (label)
public class BinaryIf : IInstruction {
    private readonly int _cond;
    private readonly int _label;
    public BinaryIf(int cond, int label) {
        _cond = cond;
        _label = label;
    }
    public int Encode() {
        return (0b1000 << 28) | (_cond << 25) | _label;
    }
}
