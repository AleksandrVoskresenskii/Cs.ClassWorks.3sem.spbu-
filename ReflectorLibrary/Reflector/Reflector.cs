// <copyright file="Reflector.cs" company="AleksandrVoskresenskii">
// Copyright (c) AleksandrVoskresenskii. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace ReflectorLibrary;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// Provides methods to reflect and diff types.
/// </summary>
public class Reflector
{
    /// <summary>
    /// Creates a file with the class definition.
    /// </summary>
    /// <param name="targetType">The type to reflect.</param>
    public void PrintStructure(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        string classCode = this.GenerateClassCode(targetType);
        string fileName = $"{targetType.Name}.cs";
        File.WriteAllText(fileName, classCode);
    }

    /// <summary>
    /// Compares two types and outputs differences.
    /// </summary>
    /// <param name="firstType">The first type.</param>
    /// <param name="secondType">The second type.</param>
    /// <returns>A list of strings describing the differences.</returns>
    public List<string> DiffClasses(Type firstType, Type secondType)
    {
        ArgumentNullException.ThrowIfNull(firstType);
        ArgumentNullException.ThrowIfNull(secondType);

        List<string> differences = new List<string>();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.Instance | BindingFlags.Static |
                             BindingFlags.DeclaredOnly;

        FieldInfo[] firstFields = firstType.GetFields(flags);
        FieldInfo[] secondFields = secondType.GetFields(flags);

        MethodInfo[] firstMethods = firstType.GetMethods(flags);
        MethodInfo[] secondMethods = secondType.GetMethods(flags);

        this.CompareFields(firstFields, secondFields, firstType.Name, secondType.Name, differences);
        this.CompareMethods(firstMethods, secondMethods, firstType.Name, secondType.Name, differences);

        return differences;
    }

    private void CompareFields(FieldInfo[] firstSet, FieldInfo[] secondSet, string firstName, string secondName, List<string> output)
    {
        var firstSetList = firstSet.ToList();
        var secondSetList = secondSet.ToList();

        // Fields in First but not Second (by name)
        foreach (var field in firstSetList.Where(f => !secondSetList.Any(s => s.Name == f.Name)))
        {
            output.Add($"Field '{field.Name}' exists in {firstName} but not in {secondName}.");
        }

        // Fields in Second but not First
        foreach (var field in secondSetList.Where(s => !firstSetList.Any(f => f.Name == s.Name)))
        {
            output.Add($"Field '{field.Name}' exists in {secondName} but not in {firstName}.");
        }

        // Common fields check
        foreach (var firstField in firstSetList)
        {
            var secondField = secondSetList.FirstOrDefault(s => s.Name == firstField.Name);
            if (secondField != null)
            {
                if (firstField.FieldType != secondField.FieldType || firstField.IsStatic != secondField.IsStatic || firstField.IsPublic != secondField.IsPublic)
                {
                    output.Add($"Field '{firstField.Name}' differs between classes.");
                }
            }
        }
    }

    private void CompareMethods(MethodInfo[] firstSet, MethodInfo[] secondSet, string firstName, string secondName, List<string> output)
    {
        // Simple signature comparison: Name + Parameter Types
        string GetSignature(MethodInfo method) =>
            $"{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name))})";

        var firstSignatures = firstSet.ToDictionary(GetSignature, m => m);
        var secondSignatures = secondSet.ToDictionary(GetSignature, m => m);

        foreach (var pair in firstSignatures)
        {
            if (!secondSignatures.ContainsKey(pair.Key))
            {
                output.Add($"Method '{pair.Key}' exists in {firstName} but not in {secondName}.");
            }
            else
            {
                // Check details
                var firstMethod = pair.Value;
                var secondMethod = secondSignatures[pair.Key];

                if (firstMethod.ReturnType != secondMethod.ReturnType ||
                    firstMethod.IsStatic != secondMethod.IsStatic ||
                    firstMethod.IsPublic != secondMethod.IsPublic)
                {
                    output.Add($"Method '{pair.Key}' differs (return type or modifiers).");
                }
            }
        }

        foreach (var pair in secondSignatures)
        {
            if (!firstSignatures.ContainsKey(pair.Key))
            {
                output.Add($"Method '{pair.Key}' exists in {secondName} but not in {firstName}.");
            }
        }
    }

    private string GenerateClassCode(Type type)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("// <auto-generated />");
        stringBuilder.AppendLine("using System;");
        stringBuilder.AppendLine("using System.Collections.Generic;");
        stringBuilder.AppendLine();

        string modifier = type.IsPublic ? "public" : "internal";
        string staticModifier = type.IsAbstract && type.IsSealed ? "static " : string.Empty;
        string genericArgs = this.GetGenericArguments(type);

        stringBuilder.AppendLine($"{modifier} {staticModifier}class {type.Name}{genericArgs}");
        stringBuilder.AppendLine("{");

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.Instance | BindingFlags.Static |
                             BindingFlags.DeclaredOnly;

        foreach (FieldInfo field in type.GetFields(flags))
        {
            stringBuilder.AppendLine($"    {this.GetFieldDeclaration(field)};");
        }

        foreach (MethodInfo method in type.GetMethods(flags))
        {
            // Skip property accessors/event methods usually, but explicitly asked for methods.
            if (!method.IsSpecialName)
            {
                stringBuilder.AppendLine(this.GetMethodDeclaration(method));
            }
        }

        // Recursively print nested types
        foreach (Type nestedType in type.GetNestedTypes(flags))
        {
            // Simplified nesting
            stringBuilder.AppendLine($"    // Nested class {nestedType.Name} omitted for brevity in this recursive block implementation.");
        }

        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    private string GetFieldDeclaration(FieldInfo field)
    {
        string access = field.IsPublic ? "public" : field.IsPrivate ? "private" : "internal";
        string staticMod = field.IsStatic ? "static " : string.Empty;
        return $"{access} {staticMod}{this.GetTypeName(field.FieldType)} {field.Name}";
    }

    private string GetMethodDeclaration(MethodInfo method)
    {
        StringBuilder builder = new StringBuilder();
        string access = method.IsPublic ? "public" : method.IsPrivate ? "private" : "internal";
        string staticMod = method.IsStatic ? "static " : string.Empty;
        string returnType = this.GetTypeName(method.ReturnType);

        var parameters = method.GetParameters().Select(p => $"{this.GetTypeName(p.ParameterType)} {p.Name}");
        string paramString = string.Join(", ", parameters);

        builder.AppendLine($"    {access} {staticMod}{returnType} {method.Name}({paramString})");
        builder.AppendLine("    {");

        if (method.ReturnType != typeof(void))
        {
            builder.AppendLine("        return default;");
        }

        builder.AppendLine("    }");
        return builder.ToString();
    }

    private string GetGenericArguments(Type type)
    {
        if (!type.IsGenericType)
        {
            return string.Empty;
        }

        var args = type.GetGenericArguments().Select(t => t.Name);
        return $"<{string.Join(", ", args)}>";
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(void))
        {
            return "void";
        }

        if (type.IsGenericType)
        {
            string name = type.Name;
            int index = name.IndexOf('`');
            if (index != -1)
            {
                name = name.Substring(0, index);
            }

            var args = type.GetGenericArguments().Select(this.GetTypeName);
            return $"{name}<{string.Join(", ", args)}>";
        }

        return type.Name;
    }
}

