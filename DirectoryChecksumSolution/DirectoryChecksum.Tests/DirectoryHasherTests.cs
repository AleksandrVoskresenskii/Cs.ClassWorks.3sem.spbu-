// MIT License
// 
// Copyright (c) 2025 AleksandrVoskresenskii
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction...
// 
// Full license text is available in the LICENSE file.

namespace DirectoryChecksum.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Набор тестов для класса <see cref="DirectoryChecksum.DirectoryHasher"/>.
    /// </summary>
    public sealed class DirectoryHasherTests
    {
        [Fact]
        public async Task ComputeFileHashAsync_ReturnsExpectedHash()
        {
            using TemporaryDirectory tempDir = new TemporaryDirectory();

            string filePath = Path.Combine(tempDir.Path, "test.txt");
            const string content = "Hello, world!";

            File.WriteAllText(filePath, content);

            byte[] actualHash = await DirectoryChecksum.DirectoryHasher
                .ComputeFileHashAsync(filePath);

            byte[] nameBytes = Encoding.UTF8.GetBytes("test.txt");
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] combined = new byte[nameBytes.Length + contentBytes.Length];
            Buffer.BlockCopy(nameBytes, 0, combined, 0, nameBytes.Length);
            Buffer.BlockCopy(contentBytes, 0, combined, nameBytes.Length, contentBytes.Length);

            using MD5 md5 = MD5.Create();
            byte[] expectedHash = md5.ComputeHash(combined);

            Assert.True(actualHash.SequenceEqual(expectedHash));
        }

        [Fact]
        public async Task SequentialAndParallelDirectoryHash_AreEqual()
        {
            using TemporaryDirectory tempDir = new TemporaryDirectory();

            File.WriteAllText(Path.Combine(tempDir.Path, "a.txt"), "Alpha");
            File.WriteAllText(Path.Combine(tempDir.Path, "b.txt"), "Beta");

            string subDirectory = Path.Combine(tempDir.Path, "sub");
            Directory.CreateDirectory(subDirectory);
            File.WriteAllText(Path.Combine(subDirectory, "c.txt"), "Gamma");

            byte[] sequential = await DirectoryChecksum.DirectoryHasher
                .ComputeDirectoryHashSequentialAsync(tempDir.Path);
            byte[] parallel = await DirectoryChecksum.DirectoryHasher
                .ComputeDirectoryHashParallelAsync(tempDir.Path);

            Assert.True(sequential.SequenceEqual(parallel));
        }

        [Fact]
        public async Task DirectoryHash_IsDeterministic()
        {
            using TemporaryDirectory tempDir = new TemporaryDirectory();

            File.WriteAllText(Path.Combine(tempDir.Path, "c.txt"), "C");
            File.WriteAllText(Path.Combine(tempDir.Path, "a.txt"), "A");
            File.WriteAllText(Path.Combine(tempDir.Path, "b.txt"), "B");

            byte[] first = await DirectoryChecksum.DirectoryHasher
                .ComputeDirectoryHashSequentialAsync(tempDir.Path);

            // Ничего по сути не меняем, просто чуть дёргаем файловую систему.
            File.AppendAllText(Path.Combine(tempDir.Path, "b.txt"), string.Empty);

            byte[] second = await DirectoryChecksum.DirectoryHasher
                .ComputeDirectoryHashSequentialAsync(tempDir.Path);

            Assert.True(first.SequenceEqual(second));
        }
    }
}
