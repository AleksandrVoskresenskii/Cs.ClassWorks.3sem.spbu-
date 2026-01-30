// <copyright file="Program.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using SymmetricListApp.Source;

namespace SymmetricListApp;

internal class Program
{
    private static void Main(string[] args)
    {
        string inputFilePath = "input.txt";

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Error: File '{inputFilePath}' not found.");
            return;
        }

        try
        {
            CustomDoublyLinkedList numberList = new();

            foreach (string line in File.ReadLines(inputFilePath))
            {
                string[] numberStrings = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string numberString in numberStrings)
                {
                    if (int.TryParse(numberString, out int number))
                    {
                        numberList.AddLast(number);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: '{numberString}' is skipped (not an integer).");
                    }
                }
            }

            bool isSymmetric = numberList.IsSymmetric();

            if (isSymmetric)
            {
                Console.WriteLine("The list is symmetric.");
            }
            else
            {
                Console.WriteLine("The list is NOT symmetric.");
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
        }
    }
}
