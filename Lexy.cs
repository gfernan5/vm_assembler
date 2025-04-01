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
// Needs a default!
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