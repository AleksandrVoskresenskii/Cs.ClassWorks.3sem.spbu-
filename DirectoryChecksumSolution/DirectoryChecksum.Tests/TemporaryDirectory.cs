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

    /// <summary>
    /// Вспомогательный класс для создания временной директории в тестах.
    /// </summary>
    public sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TemporaryDirectory"/>.
        /// Создаёт уникальную папку в системном временном каталоге.
        /// </summary>
        public TemporaryDirectory()
        {
            string tempRoot = System.IO.Path.GetTempPath();
            string directoryName = Guid.NewGuid().ToString(
                "N",
                System.Globalization.CultureInfo.InvariantCulture);

            this.Path = System.IO.Path.Combine(tempRoot, directoryName);
            Directory.CreateDirectory(this.Path);
        }

        /// <summary>
        /// Получает путь к созданной временной директории.
        /// </summary>
        public string Path { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(this.Path))
                {
                    Directory.Delete(this.Path, true);
                }
            }
            catch
            {
                // В тестах игнорируем ошибки удаления.
            }
        }
    }
}
