// Exit Instruction
public class Exit : IInstruction {
    private readonly int _exitCode;
    public Exit(int exitCode) {
        _exitCode = exitCode;
    }
    public int Encode() {
        return (0b0000 << 24) | _exitCode;
    }
}

// Swap Instruction
public class Swap : IInstruction {
    private readonly int _fromTo;
    public Swap(int fromTo) {
        _exitCode = fromTo;
    }
    public int Encode() {
        return (0b0001 << 24) | _fromTo;
    }
}

// NOP Instruction
public class NOP : IInstruction {
    private readonly int _sbz;
    public NOP(int sbz) {
        _sbz = sbz;
    }
    public int Encode() {
        return (0b0010 << 24) | _sbz;
    }
}

// Input Instruction
public class Input : IInstruction {
    private readonly int _sbz;
    public Input(int sbz) {
        _sbz = sbz;
    }
    public int Encode() {
        return (0b0100 << 24) | _sbz
    }
}

// String Input Instruction
public class Stinput : IInstruction {
    private readonly int _unsignedChars;
    public Stinput(int unsignedChars) {
        if (unsignedChars < 0 || unsignedCharsChars > 0x00FF_FFFF) {
            unsignedChars = 0x00FF_FFFF;
        }
        _unsignedChars = unsignedChars;
    }
    public int Encode() {
        return (0b0101 << 24) | _unsignedChars;
    }
}