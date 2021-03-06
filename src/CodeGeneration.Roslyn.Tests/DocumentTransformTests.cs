﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using Xunit;

public class DocumentTransformTests : CompilationTestsBase
{

    [Fact]
    public void EmptyFile_NoGenerators()
    {
        AssertGeneratedAsExpected("", "");
    }

    [Fact]
    public void Usings_WhenNoCode_CopiedToOutput()
    {
        const string usings = "using System;";
        AssertGeneratedAsExpected(usings, usings);
    }

    [Fact]
    public void AncestorTree_IsBuiltProperly()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

[EmptyPartial]
partial class Empty {}

namespace Testing.Middle
{
    using System.Linq;

    namespace Inner
    {
        partial class OuterClass<T>
        {
            partial struct InnerStruct<T1, T2>
            {
                [EmptyPartial]
                int Placeholder { get; }
            }
        }
    }
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

partial class Empty
{
}

namespace Testing.Middle
{
    using System.Linq;

    namespace Inner
    {
        partial class OuterClass<T>
        {
            partial struct InnerStruct<T1, T2>
            {
            }
        }
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void DefineDirective_Dropped()
    {
        // define directives must be leading any other tokens to be valid in C#
        const string source = @"
#define SOMETHING
using System;
using System.Linq;";
        const string generated = @"
using System;
using System.Linq;";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Comment_BetweenUsings_Dropped()
    {
        const string source = @"
using System;
// one line comment
using System.Linq;";
        const string generated = @"
using System;
using System.Linq;";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Region_TrailingUsings_Dropped()
    {
        const string source = @"
using System;
#region CustomRegion
using System.Linq;
#endregion //CustomRegion";
        const string generated = @"
using System;
using System.Linq;";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void IfElseDirective_OnUsings_InactiveUsingAndDirectives_Dropped()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;
#if SOMETHING
using System.Linq;
#else
using System.Diagnostics;
#endif

[EmptyPartial]
partial class Empty {}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;
using System.Diagnostics;

partial class Empty
{
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void RegionDirective_InsideClass_Dropped()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

partial class Empty
{
#region SomeRegion
    [EmptyPartial]
    int Counter { get; }
#endregion
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

partial class Empty
{
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void RegionDirective_InsideStruct_Dropped()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

partial struct Empty
{
#region SomeRegion
    [EmptyPartial]
    int Counter { get; }
#endregion
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

partial struct Empty
{
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void RegionDirective_InsideNamespace_Dropped()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
#region SomeRegion
    [EmptyPartial]
    partial class Empty { }
#endregion
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    partial class Empty
    {
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Class_Modifiers_ArePreserved_WithoutTrivia()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    // some one-line comment
    public static partial class Empty
    {
        [EmptyPartial]
        public static int Method() => 0;
    }
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    public static partial class Empty
    {
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Struct_Modifiers_ArePreserved_WithoutTrivia()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    // some one-line comment
    internal partial struct Empty
    {
        [EmptyPartial]
        public static int Method() => 0;
    }
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    internal partial struct Empty
    {
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Class_TypeParameters_ArePreserved()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    partial class Empty<T> where T : class
    {
        [EmptyPartial]
        public static T Method() => null;
    }
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    partial class Empty<T>
    {
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }

    [Fact]
    public void Struct_TypeParameters_ArePreserved()
    {
        const string source = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    partial struct Empty<T> where T : class
    {
        [EmptyPartial]
        public static T Method() => null;
    }
}";
        const string generated = @"
using System;
using CodeGeneration.Roslyn.Tests.Generators;

namespace Testing
{
    partial struct Empty<T>
    {
    }
}";
        AssertGeneratedAsExpected(source, generated);
    }
}
