/* 
 * File: Main.cs
 * Authors: David Long, Ethan Miller, Gian Fernandez, Lexy Andershock
 * Date: 2025-04-04
 *
 * Brief: Assembler starting point.
 */

using System;
using System.IO;
using System.Collections.Generic;

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

            _instructionList.Add(line);
            //Console.WriteLine(line);

            // update counters
            programCounter += 4;
            lineCounter++;
        }

        // DEBUG: print instructions
        // foreach (var v in _instructionList) {
        //     Console.WriteLine(v);
        // }

        foreach (var s in _instructionList) {
            string[] inst = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string instName = inst[0].ToLower();

            IInstruction instruction = null;
            switch (instName.ToLower()) {
                case "exit":
                    instruction = new Exit();
                    break;
                case "swap":
                    instruction = new Swap();
                    break;
                case "nop":
                    instruction = new Nop();
                    break;
                case "input":
                    instruction = new Input();
                    break;
                case "stinput":
                    if (inst.Length == 2 && int.TryParse(inst[1], out int value)) {
                        instruction = new Stinput(value);
                        break;
                    } else {
                        instruction = new Stinput(0x00FF_FFFF);
                        break;
                    }
                case "debug":
                    instruction = new Debug();
                    break;
                case "pop":
                    instruction = new Pop();
                    break;
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
                    instruction = new Stprint();
                    break;
                // case "call":
                //     instruction = new Call();
                //     break;
                // case "return":
                //     instruction = new Return();
                //     break;
                // case "goto":
                //     instruction = new Goto();
                //     break;
                // needs all forms and unary and binary!
                // case "if":
                //     instruction = new If();
                //     break;
                // case "dup":
                //     instruction = new Dup();
                //     break;
                case "print":
                    instruction = new Print();
                    break;
                case "dump":
                    instruction = new Dump();
                    break;
                // case "push":
                //     instruction = new Push();
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