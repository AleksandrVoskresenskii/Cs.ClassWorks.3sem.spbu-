// <copyright file="CustomDoublyLinkedListTests.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using SymmetricListApp.Source;

namespace SymmetricListApp.Tests;

[TestFixture]
public class CustomDoublyLinkedListTests
{
    private CustomDoublyLinkedList? linkedList;

    [SetUp]
    public void Setup()
    {
        this.linkedList = new CustomDoublyLinkedList();
    }

    [Test]
    public void IsSymmetric_EmptyList_ReturnsTrue()
    {
        Assert.That(this.linkedList!.IsSymmetric(), Is.True);
    }

    [Test]
    public void IsSymmetric_SingleElement_ReturnsTrue()
    {
        this.linkedList!.AddLast(1);
        Assert.That(this.linkedList.IsSymmetric(), Is.True);
    }

    [Test]
    public void IsSymmetric_TwoIdenticalElements_ReturnsTrue()
    {
        this.linkedList!.AddLast(10);
        this.linkedList.AddLast(10);
        Assert.That(this.linkedList.IsSymmetric(), Is.True);
    }

    [Test]
    public void IsSymmetric_TwoDifferentElements_ReturnsFalse()
    {
        this.linkedList!.AddLast(10);
        this.linkedList.AddLast(20);
        Assert.That(this.linkedList.IsSymmetric(), Is.False);
    }

    [Test]
    public void IsSymmetric_OddCountSymmetric_ReturnsTrue()
    {
        // 1 -> 2 -> 3 -> 2 -> 1
        this.linkedList!.AddLast(1);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(3);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(1);

        Assert.That(this.linkedList.IsSymmetric(), Is.True);
    }

    [Test]
    public void IsSymmetric_EvenCountSymmetric_ReturnsTrue()
    {
        // 1 -> 2 -> 2 -> 1
        this.linkedList!.AddLast(1);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(1);

        Assert.That(this.linkedList.IsSymmetric(), Is.True);
    }

    [Test]
    public void IsSymmetric_NotSymmetricExample_ReturnsFalse()
    {
        // 1 -> 2 -> 3 -> 45 -> 2 -> 1
        this.linkedList!.AddLast(1);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(3);
        this.linkedList.AddLast(45);
        this.linkedList.AddLast(2);
        this.linkedList.AddLast(1);

        Assert.That(this.linkedList.IsSymmetric(), Is.False);
    }
}
