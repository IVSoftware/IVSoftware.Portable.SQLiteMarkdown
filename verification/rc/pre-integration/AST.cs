/*
 * Copyright (c) 2024 IVSoftware, LLC
 * Author: Thomas C. Gregor
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    // Enum to represent the types of nodes in the AST
    public enum NodeType
    {
        Term,
        And,
        Or,
        Not,
        Parenthesis,
        Tag,
    }

    [Flags]
    public enum NodeTypeFlags
    {
        Term = 0x1,
        Tag = 0x2,
        All = 0x3,
    }

    public enum StringCasing
    {
        Original,  // Leave the string as-is
        Lower,     // Convert the string to lowercase
        Upper      // Convert the string to uppercase (if needed)
    }

    // Class representing a node in the AST
    public class ASTNode
    {
        public ASTNode(NodeType type, string value)
        {
            ASTType = type;
            Value = value;
        }

        public NodeType ASTType { get; set; }
        public string Value { get; set; }
        public List<ASTNode> Children { get; set; } = new List<ASTNode>();
        public override string ToString()
        {
            return Value;
        }
    }
}
