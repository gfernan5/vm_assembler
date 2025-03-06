/* 
 * File: Instructions.cs
 * Authors: David Long, Ethan Miller, Gian Fernandez, Lexy Andershock
 * Date: 2025-04-04
 *
 * Brief: Define classes for each instruction.
 */

public class Dup : IInstruction {
    private readonly int _offset;
    public Dup(int offset) {
        _offset = offset & ~3;
    }
    public int Encode() {
        return (0b1100 << 28) | _offset;
    }
}