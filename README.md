# Local Lobby Server

This repository contains the **source code** for the Local Lobby Server used by the Unity package  
[`com.radtad2.lobbyservice.localserver`](TODO).

---

## 🔧 Building from Source

This server is **not intended to be built directly**.  
For most users, the recommended way to use it is through the compiled binary automatically downloaded by the Unity package.

The source is provided for **transparency** and **auditing** purposes only.

If you still wish to build the server manually:

1. Copy the shared runtime code from your Unity project in com.radtad2.lobbyservice.localserver/Runtime/Shared

2. Paste it into this repository under the root.

3. Then build using the .NET CLI:
```bash
dotnet build -c Release
```

or run directly with 

```bash
dotnet run
```