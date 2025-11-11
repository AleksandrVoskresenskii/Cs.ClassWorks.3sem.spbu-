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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Предоставляет методы для вычисления детерминированных MD5-хешей файлов и каталогов.
    /// </summary>
    public static class DirectoryHasher
    {
        /// <summary>
        /// Асинхронно вычисляет MD5-хеш файла.
        /// f(файл) = MD5(имя_файла_UTF8 + содержимое_файла).
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <returns>Массив из 16 байт — MD5-хеш.</returns>
        public static async Task<byte[]> ComputeFileHashAsync(string filePath)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string fileName = Path.GetFileName(filePath);
            byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);

            byte[] contentBytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

            byte[] combined = new byte[nameBytes.Length + contentBytes.Length];
            Buffer.BlockCopy(nameBytes, 0, combined, 0, nameBytes.Length);
            Buffer.BlockCopy(contentBytes, 0, combined, nameBytes.Length, contentBytes.Length);

            using MD5 md5 = MD5.Create();
            return md5.ComputeHash(combined);
        }

        /// <summary>
        /// Синхронная обёртка для <see cref="ComputeFileHashAsync"/>.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <returns>Массив из 16 байт — MD5-хеш.</returns>
        public static byte[] ComputeFileHash(string filePath)
        {
            return ComputeFileHashAsync(filePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Асинхронно вычисляет хеш каталога однопоточным способом.
        /// </summary>
        /// <param name="directoryPath">Полный путь к каталогу.</param>
        /// <returns>Массив из 16 байт — MD5-хеш каталога.</returns>
        public static Task<byte[]> ComputeDirectoryHashSequentialAsync(string directoryPath)
        {
            return ComputeDirectoryHashAsync(directoryPath, false);
        }

        /// <summary>
        /// Асинхронно вычисляет хеш каталога, обрабатывая детей параллельно.
        /// </summary>
        /// <param name="directoryPath">Полный путь к каталогу.</param>
        /// <returns>Массив из 16 байт — MD5-хеш каталога.</returns>
        public static Task<byte[]> ComputeDirectoryHashParallelAsync(string directoryPath)
        {
            return ComputeDirectoryHashAsync(directoryPath, true);
        }

        /// <summary>
        /// Синхронная обёртка для <see cref="ComputeDirectoryHashSequentialAsync"/>.
        /// </summary>
        /// <param name="directoryPath">Полный путь к каталогу.</param>
        /// <returns>MD5-хеш каталога.</returns>
        public static byte[] ComputeDirectoryHashSequential(string directoryPath)
        {
            return ComputeDirectoryHashSequentialAsync(directoryPath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Синхронная обёртка для <see cref="ComputeDirectoryHashParallelAsync"/>.
        /// </summary>
        /// <param name="directoryPath">Полный путь к каталогу.</param>
        /// <returns>MD5-хеш каталога.</returns>
        public static byte[] ComputeDirectoryHashParallel(string directoryPath)
        {
            return ComputeDirectoryHashParallelAsync(directoryPath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Преобразует байтовый хеш в шестнадцатеричное строковое представление.
        /// </summary>
        /// <param name="hash">Массив байт.</param>
        /// <returns>Строка в верхнем регистре без разделителей.</returns>
        public static string HashToString(byte[] hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var builder = new StringBuilder(hash.Length * 2);

            foreach (byte value in hash)
            {
                builder.Append(value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Внутренний метод для вычисления хеша каталога.
        /// </summary>
        /// <param name="directoryPath">Путь к каталогу.</param>
        /// <param name="parallel">
        /// Если true, дочерние элементы обрабатываются параллельно с помощью задач.
        /// </param>
        /// <returns>Массив из 16 байт — MD5-хеш.</returns>
        private static async Task<byte[]> ComputeDirectoryHashAsync(string directoryPath, bool parallel)
        {
            if (directoryPath is null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            var directoryInfo = new DirectoryInfo(directoryPath);
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException($"Directory '{directoryPath}' does not exist.");
            }

            byte[] nameBytes = Encoding.UTF8.GetBytes(directoryInfo.Name);

            var entries = new List<string>();
            entries.AddRange(Directory.GetDirectories(directoryPath));
            entries.AddRange(Directory.GetFiles(directoryPath));

            // Детеминированность: сортируем по имени.
            entries.Sort(StringComparer.Ordinal);

            var childTasks = new List<Task<byte[]>>(entries.Count);

            foreach (string entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    if (parallel)
                    {
                        childTasks.Add(Task.Run(() => ComputeDirectoryHashAsync(entry, true)));
                    }
                    else
                    {
                        childTasks.Add(ComputeDirectoryHashAsync(entry, false));
                    }
                }
                else if (File.Exists(entry))
                {
                    if (parallel)
                    {
                        childTasks.Add(Task.Run(() => ComputeFileHashAsync(entry)));
                    }
                    else
                    {
                        childTasks.Add(ComputeFileHashAsync(entry));
                    }
                }
            }

            byte[][] childHashes = await Task.WhenAll(childTasks).ConfigureAwait(false);

            int totalLength = nameBytes.Length + childHashes.Sum(hash => hash.Length);
            byte[] combined = new byte[totalLength];

            int offset = 0;
            Buffer.BlockCopy(nameBytes, 0, combined, offset, nameBytes.Length);
            offset += nameBytes.Length;

            foreach (byte[] childHash in childHashes)
            {
                Buffer.BlockCopy(childHash, 0, combined, offset, childHash.Length);
                offset += childHash.Length;
            }

            using MD5 md5 = MD5.Create();
            return md5.ComputeHash(combined);
        }
    }
}
