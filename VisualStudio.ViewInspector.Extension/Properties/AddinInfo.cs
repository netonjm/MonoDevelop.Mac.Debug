using System.Runtime.Versioning;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
	"ViewInspector",
	Namespace = "VisualStudio",
	Version = "0.8.3"
)]

[assembly: AddinName("VisualStudio View Inspector")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Allows enable/disable inspect UI from the IDE")]
[assembly: AddinAuthor("Jose Medrano")]

// Need to fix CA1416 build warning.
// This call site is reachable on all platforms. 'NSLayoutConstraint.Active' is only supported on: 'ios' 10.0 and later,
// 'maccatalyst' 10.0 and later, 'macOS/OSX' 10.14 and later, 'tvos' 10.0 and later. (CA1416))
// https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
[assembly: SupportedOSPlatform("macos10.15")] 