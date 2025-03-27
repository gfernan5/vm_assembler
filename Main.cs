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
        string fileName = args[0]?.Trim() ?? string.Empty;

        try {
            sr = new StreamReader(fileName);
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
            Console.WriteLine(line);

            programCounter += 4;
            lineCounter++;
        }
    }
}