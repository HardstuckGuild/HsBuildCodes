# Hardstuck BuildCodes V2 - C# 10 implementation

Everything can be directly included with
```csharp
using Hardstuck.GuildWars2.BuildCodes.V2;
```

### There are multiple ways to obtain a Hardstuck BuildCode Object:
```csharp
PerProfessionData.ReloadAll();

// From existing build textual codes:
var code1 = TextLoader.LoadBuildCode("CoI___~______A~M~__A_");

// From existing build binary codes:
ReadonlySpan<byte> bytes = SomeArbitrarySource();
var code2 = BinaryLoader.LoadBuildCode(bytes);

// From existing textual in-game chatlinks:
var inGameLink = "[&DQkAAAAARQDcEdwRAAAAACsSAADUEQAAAAAAAAQCAwDUESsSAAAAAAAAAAA=]";
var code3 = TextLoader.LoadOfficialBuildCode(inGameLink);

// From existing binary in-game chatlink data:
var rawInGameBytes = Convert.FromBase64String(inGameLink[2..^1]);
var code4 = BinaryLoader.LoadOfficialBuildCode(rawInGameBytes);

// From an existing player via the official api:
var apiKey = "AD041D99-AEEF...";
var characterName = "Don Joe Was Taken";
var code5 = await APILoader.LoadBuildCode(apiKey, characterName, Kind.PvE);

// From the character currently ingame on the same machine:
var code6 = await APILoader.LoadBuildCodeFromCurrentCharacter(apiKey);
```

### From this Object different kinds of codes can be generated:

```csharp
// To a textual version of the Hardstuck BuildCode
string hsCode = TextLoader.WriteBuildCode(code);

// To a compressed binary version of the Hardstuck BuildCode
var buffer1 = new byte[500];
int bytesWritten1 = BinaryLoader.WriteBuildCode(code, buffer1);

// Directly to a full in-game chat link
string chatLink = TextLoader.WriteOfficialBuildCode(code);

// To the raw data of an in-game chat link
var buffer2 = new byte[500];
int bytesWritten2 = BinaryLoader.WriteOfficialBuildCode(code, buffer2);
```

There are multiple overloads available for most of those operations.