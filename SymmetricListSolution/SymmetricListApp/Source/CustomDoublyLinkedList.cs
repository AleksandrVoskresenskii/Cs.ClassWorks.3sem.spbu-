// <copyright file="CustomDoublyLinkedList.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SymmetricListApp.Source;

public class CustomDoublyLinkedList
{
    private CustomLinkedListNode? head;
    private CustomLinkedListNode? tail;

    public void AddLast(int value)
    {
        CustomLinkedListNode newNode = new(value);

        if (this.tail == null)
        {
            this.head = newNode;
            this.tail = newNode;
        }
        else
        {
            this.tail.Next = newNode;
            newNode.Previous = this.tail;
            this.tail = newNode;
        }
    }

    public bool IsSymmetric()
    {
        if (this.head == null)
        {
            return true;
        }

        CustomLinkedListNode? leftPointer = this.head;
        CustomLinkedListNode? rightPointer = this.tail;

        while (leftPointer != rightPointer && leftPointer?.Previous != rightPointer)
        {
            if (leftPointer!.Value != rightPointer!.Value)
            {
                return false;
            }

            leftPointer = leftPointer.Next;
            rightPointer = rightPointer.Previous;
        }

        return true;
    }
}
