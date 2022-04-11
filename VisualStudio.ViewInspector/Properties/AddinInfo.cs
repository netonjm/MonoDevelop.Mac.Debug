//
// AddinInfo.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2017 Microsoft Corp.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.Versioning;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
	"VisualStudio.ViewInspector",
	Namespace = "VisualStudio.ViewInspector",
	Version = "1.1"
)]

[assembly: AddinName("VisualStudio.ViewInspector")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("VisualStudio.ViewInspector")]
[assembly: AddinAuthor("Jose Medrano")]

// Need to fix CA1416 build warning.
// This call site is reachable on all platforms. 'NSLayoutConstraint.Active' is only supported on: 'ios' 10.0 and later,
// 'maccatalyst' 10.0 and later, 'macOS/OSX' 10.14 and later, 'tvos' 10.0 and later. (CA1416))
// https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
[assembly: SupportedOSPlatform("macos10.15")] 