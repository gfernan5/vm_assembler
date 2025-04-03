/* 
 * File: Main.cs
 * Authors: David Long, Ethan Miller, Gian Fernandez, Lexy Andershock
 * Date: 2025-04-04
 *
 * Brief: 2-pass Assembler in C# intended for a virtual machine in Rust.
 */

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

class Assembler
{
    public static void Main(string[] args)
    {
        int programCounter = 0;
        int lineCounter = 0;

        Dictionary<string, int> labelMap = new Dictionary<string, int>();
        List<string> instructionList = new List<string>();
        List<IInstruction> instructionInterfaces = new List<IInstruction>();

        // check CMD line arguments
        if (args.Length != 2) {
            Console.Error.WriteLine("Usage: assemble <file.asm> <file.v>");
            return;
        }

        // open asm file and set up a StreamReader
        StreamReader sr;
        string inputFileName = args[0]?.Trim() ?? string.Empty;

        // file error checking
        try {
            sr = new StreamReader(inputFileName);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            return;
        }

        /* PASS 1 */
        string line;
        while ((line = sr.ReadLine()) != null) {
            // clear all comments and trim whitespace
            line = line.Split('#')[0].Trim();

            // skip stpush instructions
            if (!line.TrimStart().StartsWith("stpush")) {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                line = string.Join(" ", parts);
            }

            // skip any empty lines
            if (string.IsNullOrEmpty(line)) {
                continue;
            }

            // process labels
            if (line.EndsWith(':')) {
                string label = line.Substring(0, line.Length - 1);
                labelMap[label] = programCounter;
                continue;
            }

            // check / expand stpush - do not add to _instruction list
            Stack<string> stack = new Stack<string>();
            if (line.Contains("stpush")) {
                // Find the index of the first and last quotes
                int startIndex = line.IndexOf('"') + 1; 
                int endIndex = line.LastIndexOf('"');
                // grab substring of this value
                string input = Regex.Unescape(line.Substring(startIndex, endIndex - startIndex));
                
                // 1. turn input into binary string
                byte[] bytes = Encoding.ASCII.GetBytes(input);
                // 2. Process the string in chunks of 3 characters
                for (int i = 0; i < bytes.Length; i += 3) {
                    int value = 0;
                    // 3. grab the appropriate number of bytes for binary (3 or less)
                    int remaining;
                    if (3 < bytes.Length - i) { 
                        remaining = 3; 
                    }
                    else { 
                        remaining = bytes.Length - i; 
                    }
                    // 4. Pack bytes into value
                    for (int j = remaining - 1; j >= 0; j--) {
                        value |= (bytes[i + j] << (16 - (j * 8)));
                    }
                    // 5. If there are more bytes after, add continuation byte. else, stop
                    if (i + 3 < bytes.Length) {
                        value = (value << 8) | (0x01);
                    }
                    else {
                        // 6. process remaining empty characters
                        int bytes_left = bytes.Length - i;
                        for (int k = 0; k < (3 - bytes_left); k++) {
                            value |= (0x01 << 8*k);
                        }
                        value = (value << 8) | (0x00);
                    }
                    byte[] bytes2 = BitConverter.GetBytes(value);
                    string push_value = BitConverter.ToString(bytes2).Replace("-", "").ToLower();
                    string final_v = "push " + "0x" + push_value;

                    stack.Push(final_v);
                    programCounter += 4;
                }
                // pop off the stack and add to _instructionList
                while (stack.Count != 0) {
                    instructionList.Add(stack.Pop());
                }
                continue;
            }
            else {
                instructionList.Add(line);
            }

            // update counters
            programCounter += 4;
            lineCounter++;
        }

        // error check number of instructions
        if (instructionList.Count == 0) {
            Console.WriteLine("error: no instructions to assemble!");
        }

        // add nop instructions to pad out to a multiple of 4
        int rem = instructionList.Count % 4;
        if (rem != 0) {
            for (int i = 0; i < (4 - rem); i++) {
                instructionList.Add("nop");
            }
        }

        // reset program counter to pass 2
        programCounter = 0;
        
        /* PASS 2 */
        foreach (var s in instructionList) {
            string[] inst = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string instName = inst[0].ToLower();

            // switch on instruction names
            // process any instruction arguments
            // create IInstruction interfaces for each instruction
            IInstruction instruction = null;
            switch (instName) {
                case "exit":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int exitCode)) {
                        instruction = new Exit(exitCode);
                    } else {
                        instruction = new Exit();
                    }
                    programCounter += 4;
                    break;
                case "swap":
                    if (inst.Length >= 2) {
                        int.TryParse(inst[1], out int from);
                        if (inst.Length == 3 && int.TryParse(inst[2], out int to)) {
                            instruction = new Swap(from, to);
                            programCounter += 4;
                            break;
                        }
                        instruction = new Swap(from);
                        programCounter += 4;
                        break;
                    } else {
                        instruction = new Swap();
                        programCounter += 4;
                        break;
                    }
                case "nop":
                    instruction = new Nop();
                    programCounter += 4;
                    break;
                case "input":
                    instruction = new Input();
                    programCounter += 4;
                    break;
                case "stinput":
                    if (inst.Length == 2) {
                        int stinputValue = 0;
                        if (int.TryParse(inst[1], out int stiDecValue)) {
                            stinputValue = stiDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int stiHexValue)) {
                            stinputValue = stiHexValue;
                        }
                        instruction = new Stinput(stinputValue);
                        programCounter += 4;
                        break;
                    } else {
                        instruction = new Stinput(0x00FF_FFFF);
                        programCounter += 4;
                        break;
                    }
                case "debug":
                    if (inst.Length == 2) {
                        int debugValue = 0;
                        if (int.TryParse(inst[1], out int decValue)) {
                            debugValue = decValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue)) {
                            debugValue = hexValue;
                        }
                        instruction = new Debug(debugValue);
                        programCounter += 4;
                        break;
                    } else {
                        instruction = new Debug();
                        programCounter += 4;
                        break;
                    }
                case "pop":
                    int popValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int decPopValue)) {
                            popValue = decPopValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexPopValue)) {
                            popValue = hexPopValue;
                        }
                        instruction = new Pop(popValue);
                        programCounter += 4;
                        break;
                    } else {
                        instruction = new Pop();
                        programCounter += 4;
                        break;
                    }
                case "add":
                    instruction = new Add();
                    programCounter += 4;
                    break;
                case "sub":
                    instruction = new Sub();
                    programCounter += 4;
                    break;
                case "mul":
                    instruction = new Mul();
                    programCounter += 4;
                    break;
                case "div":
                    instruction = new Div();
                    programCounter += 4;
                    break;
                case "rem":
                    instruction = new Rem();
                    programCounter += 4;
                    break;
                case "and":
                    instruction = new And();
                    programCounter += 4;
                    break;
                case "or":
                    instruction = new Or();
                    programCounter += 4;
                    break;
                case "xor":
                    instruction = new Xor();
                    programCounter += 4;
                    break;
                case "lsl":
                    instruction = new Lsl();
                    programCounter += 4;
                    break;
                case "lsr":
                    instruction = new Lsr();
                    programCounter += 4;
                    break;
                case "asr":
                    instruction = new Asr();
                    programCounter += 4;
                    break;
                case "neg":
                    instruction = new Neg();
                    programCounter += 4;
                    break;
                case "not":
                    instruction = new Not();
                    programCounter += 4;
                    break;
                case "stprint":
                    int stprintValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int stpDecValue)) {
                            stprintValue = stpDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int stpHexValue)) {
                            stprintValue = stpHexValue;
                        }
                        instruction = new Stprint(stprintValue);
                        programCounter += 4;
                        break;
                    } else {
                        instruction = new Stprint();
                        programCounter += 4;
                        break;
                    }
                case "call":
                    instruction = new Call(programCounter, labelMap[inst[1]]);
                    programCounter += 4;
                    break;
                case "return":
                    if (inst.Length == 2) {
                        int returnValue = 0;
                        if (int.TryParse(inst[1], out int retVal)) {
                            returnValue = retVal;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rethexValue)) {
                            returnValue = rethexValue;
                        }
                        instruction = new Return(returnValue);
                    } else {
                        instruction = new Return();   
                    }
                    programCounter += 4;
                    break;
                case "goto":
                    instruction = new Goto(labelMap[inst[1]] - programCounter);
                    programCounter += 4;
                    break;
                case "ifez":
                    if (inst.Length == 2) {
                        instruction = new UnaryIf(0b00, labelMap[inst[1]] - programCounter);
                    } else {
                        instruction = new UnaryIf(0b00, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifnz":
                    if (inst.Length == 2) {
                        instruction = new UnaryIf(0b01, labelMap[inst[1]] - programCounter);
                    } else {
                        instruction = new UnaryIf(0b01, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifmi":
                    if (inst.Length == 2) {
                        instruction = new UnaryIf(0b10, labelMap[inst[1]] - programCounter);
                    } else {
                        instruction = new UnaryIf(0b10, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifpl":
                    if (inst.Length == 2) {
                        instruction = new UnaryIf(0b11, labelMap[inst[1]] - programCounter);
                    } else {
                        instruction = new UnaryIf(0b11, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifeq":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b000, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b000, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifne":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b001, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b001, 0);
                    }
                    programCounter += 4;
                    break;
                case "iflt":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b010, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b010, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifgt":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b011, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b011, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifle":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b100, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b100, 0);
                    }
                    programCounter += 4;
                    break;
                case "ifge":
                    if(inst.Length == 2) {
                        instruction = new BinaryIf(0b101, labelMap[inst[1]] - programCounter);
                    }
                    else {
                        instruction = new BinaryIf(0b101, 0);
                    }
                    programCounter += 4;
                    break;
                case "dup":
                    int dupValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int dupDecValue)) {
                            dupValue = dupDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int dupHexValue)) {
                            dupValue = dupHexValue;
                        }
                    }
                    instruction = new Dup(dupValue);
                    programCounter += 4;
                    break;
                case "print":
                    int printValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int printDecValue)) {
                            printValue = printDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int printHexValue)) {
                            printValue = printHexValue;
                        }
                    }
                    instruction = new Print(0, printValue);
                    programCounter += 4;
                    break;
                case "printh":
                    int printhValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int printhDecValue)) {
                            printhValue = printhDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int printhHexValue)) {
                            printhValue = printhHexValue;
                        }
                    }
                    instruction = new Print(1, printhValue);
                    programCounter += 4;
                    break;
                case "printo":
                    int printoValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int printoDecValue)) {
                            printoValue = printoDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int printoHexValue)) {
                            printoValue = printoHexValue;
                        }
                    }
                    instruction = new Print(3, printoValue);
                    programCounter += 4;
                    break;
                case "printb":
                    int printbValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int printbDecValue)) {
                            printbValue = printbDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int printbHexValue)) {
                            printbValue = printbHexValue;
                        }
                    }
                    instruction = new Print(2, printbValue);
                    programCounter += 4;
                    break;
                case "dump":
                    instruction = new Dump();
                    programCounter += 4;
                    break;
                case "push":
                    int pushValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int pushDecValue)) {
                            pushValue = pushDecValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int pushHexValue)) {
                            pushValue = pushHexValue;
                        } else {
                            instruction = new Push(labelMap[inst[1]]);
                            programCounter += 4;
                            break;
                        }
                    }
                    instruction = new Push(pushValue);
                    programCounter += 4;
                    break;
            }

            // add IInstruction interfaces to the instructionInterfaces list
            if (instruction != null) {
                instructionInterfaces.Add(instruction);
            }
        }

        // create output file stream
        FileStream fs;
        string outputFileName = args[1]?.Trim() ?? string.Empty;

        // create binary writer using the file stream
        fs = new FileStream(outputFileName, FileMode.Create);
        BinaryWriter bWriter = new BinaryWriter(fs);

        // write magic header to v file
        bWriter.Write((byte)0xDE);
        bWriter.Write((byte)0xAD);
        bWriter.Write((byte)0xBE);
        bWriter.Write((byte)0xEF);

        // call Encode() on each interface in the instructionInterfaces list
        foreach (var inst in instructionInterfaces) {
            bWriter.Write(inst.Encode());
        }
    }
}
