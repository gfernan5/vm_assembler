// Exit Instruction
public class Exit : IInstruction {
    private readonly int _exitCode;
    public Exit(int exitCode = 0) {
        _exitCode = exitCode & 0x00FF_FFFF;
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
public class NOP : IInstruction {
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