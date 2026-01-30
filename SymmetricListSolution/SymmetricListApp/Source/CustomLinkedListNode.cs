// <copyright file="CustomLinkedListNode.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SymmetricListApp.Source;

public class CustomLinkedListNode(int value)
{
    public int Value { get; set; } = value;
    public CustomLinkedListNode? Next { get; set; }
    public CustomLinkedListNode? Previous { get; set; }
}
