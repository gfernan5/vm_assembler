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

    public Pop(uint offset = 4) {
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
