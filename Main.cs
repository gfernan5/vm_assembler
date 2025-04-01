/* 
 * File: Main.cs
 * Authors: David Long, Ethan Miller, Gian Fernandez, Lexy Andershock
 * Date: 2025-04-04
 *
 * Brief: Assembler starting point.
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
        List<string> _instructionList = new List<string>();
        List<IInstruction> instructionList = new List<IInstruction>();

        // check CMD line arguments
        if (args.Length != 2) {
            Console.Error.WriteLine("Usage: assemble <file.asm> <file.v>");
            return;
        }

        // open asm file and set up StreamReader
        StreamReader sr;
        string inputFileName = args[0]?.Trim() ?? string.Empty;

        try {
            sr = new StreamReader(inputFileName);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            return;
        }

        // read file line by line
        string line;
        while ((line = sr.ReadLine()) != null) {
            // clear all comments and trim whitespace
            line = line.Split('#')[0].Trim();
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            line = string.Join(" ", parts);

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
                    if (3 < bytes.Length - i) { remaining = 3; }
                    else { remaining = bytes.Length - i; }
                    // 4. Pack bytes into value
                    for (int j = 0; j < remaining; j++) {
                        value |= (bytes[i + j] << (16 - (j * 8)));
                    }
                    // 5. If there are more bytes after, add continuation byte. else, stop
                    if (i + 3 < bytes.Length) {
                        value |= (0x01 << 24);
                        string new_value = $"push 0x{value:X8}";
                        _instructionList.Add(new_value);
                        programCounter += 4;
                    }
                    else {
                        value |= (0x00 << 24);
                        string new_value = $"push 0x{value:X8}";
                        _instructionList.Add(new_value);
                        programCounter += 4;
                    }
                }
                continue;
            }
            else {
                _instructionList.Add(line);
            }
            //Console.WriteLine(line);

            // update counters
            programCounter += 4;
            lineCounter++;
        }

        // DEBUG: print instructions
        // foreach (var v in _instructionList) {
        //     Console.WriteLine(v);
        // }

        // foreach (var v in labelMap) {
        //     Console.WriteLine($"key: {v.Key}: value: {v.Value}");
        // }

        foreach (var s in _instructionList) {
            string[] inst = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string instName = inst[0].ToLower();

            IInstruction instruction = null;
            switch (instName) {
                case "exit":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int exitCode)) {
                        instruction = new Exit(exitCode);
                        break;
                    } else {
                        instruction = new Exit();
                        break;
                    }
                case "swap":
                    if (inst.Length >= 2) {
                        int.TryParse(inst[1], out int from);
                        if (inst.Length == 3 && int.TryParse(inst[2], out int to)) {
                            instruction = new Swap(from, to);
                            break;
                        }
                        instruction = new Swap(from);
                        break;
                    } else {
                        instruction = new Swap();
                        break;
                    }
                case "nop":
                    instruction = new Nop();
                    break;
                case "input":
                    instruction = new Input();
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
                        break;
                    } else {
                        instruction = new Stinput(0x00FF_FFFF);
                        break;
                    }
                case "debug":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int debugValue)) {
                        instruction = new Debug(debugValue);
                        break;
                    } else {
                        instruction = new Debug();
                        break;
                    }
                case "pop":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int popOffset)) {
                        instruction = new Pop(popOffset);
                        break;
                    } else {
                        instruction = new Pop();
                        break;
                    }
                case "add":
                    instruction = new Add();
                    break;
                case "sub":
                    instruction = new Sub();
                    break;
                case "mul":
                    instruction = new Mul();
                    break;
                case "div":
                    instruction = new Div();
                    break;
                case "rem":
                    instruction = new Rem();
                    break;
                case "and":
                    instruction = new And();
                    break;
                case "or":
                    instruction = new Or();
                    break;
                case "xor":
                    instruction = new Xor();
                    break;
                case "lsl":
                    instruction = new Lsl();
                    break;
                case "lsr":
                    instruction = new Lsr();
                    break;
                case "asr":
                    instruction = new Asr();
                    break;
                case "neg":
                    instruction = new Neg();
                    break;
                case "not":
                    instruction = new Not();
                    break;
                case "stprint":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int stprintOffset)) {
                        instruction = new Stprint(stprintOffset);
                        break;
                    } else {
                        instruction = new Stprint();
                        break;
                    }
                // case "call":
                //     instruction = new Call();
                //     break;
                // case "return":
                //     instruction = new Return();
                //     break;
                case "goto":
                    instruction = new Goto(labelMap[inst[1]]);
                    Console.WriteLine(instruction.Encode());
                    Console.WriteLine(labelMap[inst[1]]);
                    break;
                // needs all forms and unary and binary!
                // case "if":
                //     instruction = new If();
                //     break;
                case "dup":
                    int dupValue = 0;
                    if (inst.Length == 2) {
                        if (int.TryParse(inst[1], out int decValue)) {
                            dupValue = decValue;
                        } else if (inst[1].StartsWith("0x") && int.TryParse(inst[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue)) {
                            dupValue = hexValue;
                        }
                    }
                    instruction = new Dup(dupValue);
                    break;
                case "print":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int printOffset)) {
                        instruction = new Print(0, printOffset);
                        break;
                    } else {
                        instruction = new Print(0, 0);
                        break;
                    }
                case "printh":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int printhOffset)) {
                        instruction = new Print(1, printhOffset);
                        break;
                    } else {
                        instruction = new Print(1, 0);
                        break;
                    }
                case "printo":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int printoOffset)) {
                        instruction = new Print(3, printoOffset);
                        break;
                    } else {
                        instruction = new Print(3, 0);
                        break;
                    }
                case "printb":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int printbOffset)) {
                        instruction = new Print(2, printbOffset);
                        break;
                    } else {
                        instruction = new Print(2, 0);
                        break;
                    }
                case "dump":
                    instruction = new Dump();
                    break;
                // case "push":
                //     if (inst.Length == 2 && int.TryParse(inst[1], out int pushValue)) {
                //         instruction = new Push(pushValue);
                //         break;
                //     } else {
                //         instruction = new Push();
                //         break;
                //     }
                //     break;
                
            }

            if (instruction != null) {
                instructionList.Add(instruction);
            }
        }

        // DEBUG: print interfaces
        // foreach (var v in instructionList) {
        //     Console.WriteLine(v);
        // }

        FileStream fs;
        string outputFileName = args[1]?.Trim() ?? string.Empty;

        fs = new FileStream(outputFileName, FileMode.Create);
        BinaryWriter bWriter = new BinaryWriter(fs);

        bWriter.Write((byte)0xDE);
        bWriter.Write((byte)0xAD);
        bWriter.Write((byte)0xBE);
        bWriter.Write((byte)0xEF);

        foreach (var inst in instructionList) {
            bWriter.Write(inst.Encode());
        }
    }
}