# Issue Title
Build Fails Due to Missing/Incorrect Microsoft.Build.Framework or WebAssembly SDK

# Description
The CI workflow fails with the following error when building NihongoBot.Client:
error MSB4061: The "Microsoft.NET.Sdk.WebAssembly.ComputeWasmBuildAssets" task could not be instantiated... Could not load file or assembly 'Microsoft.Build.Framework'... error MSB4027: The "ComputeWasmBuildAssets" task generated invalid items from the "AssetCandidates" output parameter. This is likely caused by a missing or incompatible WebAssembly SDK or Microsoft.Build.Framework version.

# Solution
- Update all relevant SDK and NuGet package references in NihongoBot.Client.csproj to the latest (matching .NET 9 preview if that's the target)
- Clear and restore NuGet caches
- Ensure the build agent uses the correct .NET SDK version.

See: [GitHub Actions Run](https://github.com/dennisblokland/NihongoBot/actions/runs/21369205106/job/61509039800)