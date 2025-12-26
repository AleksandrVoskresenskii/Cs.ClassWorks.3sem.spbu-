// MIT License
// 
// Copyright (c) 2025 AleksandrVoskresenskii
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction...
// 
// Full license text is available in the LICENSE file.

namespace DirectoryChecksum
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Входная точка консольного приложения.
    /// Запускает однопоточный и многопоточный асинхронные варианты и сравнивает время.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Асинхронный метод Main. Первый аргумент командной строки — путь к каталогу.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Задача завершения работы приложения.</returns>
        public static async Task Main(string[] args)
        {
            if (args is null || args.Length == 0)
            {
                Console.WriteLine("Usage: DirectoryChecksum <directory path>");
                return;
            }

            string path = args[0];

            if (!System.IO.Directory.Exists(path))
            {
                Console.WriteLine($"The specified directory '{path}' does not exist.");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            byte[] sequentialHash = await DirectoryHasher
                .ComputeDirectoryHashSequentialAsync(path)
                .ConfigureAwait(false);
            stopwatch.Stop();

            Console.WriteLine($"Sequential checksum: {DirectoryHasher.HashToString(sequentialHash)}");
            Console.WriteLine($"Sequential computation time: {stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms");

            stopwatch.Restart();
            byte[] parallelHash = await DirectoryHasher
                .ComputeDirectoryHashParallelAsync(path)
                .ConfigureAwait(false);
            stopwatch.Stop();

            Console.WriteLine($"Parallel checksum:   {DirectoryHasher.HashToString(parallelHash)}");
            Console.WriteLine($"Parallel computation time:   {stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms");

            if (!sequentialHash.SequenceEqual(parallelHash))
            {
                Console.WriteLine("Warning: Sequential and parallel checksums differ!");
            }
        }
    }
}
