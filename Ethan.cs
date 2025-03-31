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
