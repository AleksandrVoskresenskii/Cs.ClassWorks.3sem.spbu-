// <copyright file="ReflectorTests.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace ReflectorLibrary.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

[TestFixture]
public class ReflectorTests
{
    private Reflector reflector;
    private const string TestFileName = "TestClass.cs";

    [SetUp]
    public void Setup()
    {
        this.reflector = new Reflector();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(TestFileName))
        {
            File.Delete(TestFileName);
        }
    }

    [Test]
    public void PrintStructure_ShouldCreateFile()
    {
        this.reflector.PrintStructure(typeof(TestClass));
        Assert.That(File.Exists(TestFileName), Is.True);

        string content = File.ReadAllText(TestFileName);
        Assert.That(content, Does.Contain("class TestClass"));
        Assert.That(content, Does.Contain("public Int32 PublicField"));
        Assert.That(content, Does.Contain("private void PrivateMethod()"));
        Assert.That(content, Does.Contain("return default;"));
    }

    [Test]
    public void DiffClasses_ShouldDetectMissingMembers()
    {
        var differences = this.reflector.DiffClasses(typeof(TestClass), typeof(ModifiedTestClass));

        // TestClass has PublicField, Modified has not
        Assert.That(differences, Has.One.Matches<string>(s => s.Contains("Field 'PublicField' exists in TestClass but not in ModifiedTestClass")));

        // Modified has NewField, TestClass has not
        Assert.That(differences, Has.One.Matches<string>(s => s.Contains("Field 'NewField' exists in ModifiedTestClass but not in TestClass")));
    }

    [Test]
    public void DiffClasses_ShouldDetectSignatureDifference()
    {
        var differences = this.reflector.DiffClasses(typeof(TestClass), typeof(ModifiedTestClass));

        // SharedMethod in TestClass is void, in Modified is int
        Assert.That(differences, Has.One.Matches<string>(s => s.Contains("Method 'SharedMethod()' differs")));
    }

    // --- Test Data Classes ---

    private class TestClass
    {
        public int PublicField = 0;

        private void PrivateMethod()
        {
        }

        public void SharedMethod()
        {
        }

        // Добавляем метод с возвращаемым значением, чтобы генератор создал "return default;"
        public int MethodWithReturn()
        {
            return 0;
        }
    }

    private class ModifiedTestClass
    {
        public int NewField = 0;

        public int SharedMethod()
        {
            return 0;
        }
    }

}
