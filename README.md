# MRP: `Script class can only be set together with base class name` on hot reload

Minimal reproduction for the error that the multi-assembly C# support prints
after an assembly reload when an exported typed collection's element type is a
script class from a referenced assembly.

## Layout

| Path | What it is |
|---|---|
| `Game/` | Godot project. `project.godot` + `Game.csproj` (CrackTower SDK 4.7.2.2). |
| `Game/GameScript.cs` | A script in the game assembly. Editing/rebuilding it is what makes the editor hot-reload. |
| `Game/main.tscn` | A `Node` using `TaskRoot` from the library, with a `main_task.tres` instance assigned. Opening it keeps live instances of library types around, so the reload has to re-create them. |
| `Game/main_task.tres` | A `Resource` of a library script class. |
| `Libs/TypedArrayBug.Lib/` | Referenced assembly whose sources live outside the Godot project directory, so its classes get `csharp://` script paths. |

The library mirrors the shape that triggers the bug:

* `TaskBase` / `ConditionBase` – abstract `[GlobalClass]` resources.
* `TaskRunAll` / `TaskRunAny` / `TaskRunSerial` / `TaskPrint` /
  `ConditionRandomSelect` – subclasses that export
  `Godot.Collections.Array<TaskBase>` (or `Array<ConditionBase>`) **initialized
  inline** (`= []`), i.e. inside their constructor and in the generated
  `GetGodotPropertyDefaultValues()`.
* `TaskRoot` – a `Node` script from the library so the opened scene has an
  instance of a library type.

## Reproduce

Requires the fork editor **4.7.2.2** (the released one is fine) and the
`CrackTower.Godot.*` packages from nuget.org.

1. Build the solution once so the editor finds an up-to-date assembly:
   ```
   dotnet build Game/Game.csproj
   ```
2. Open the project in the editor and open `main.tscn` in it.
   ```
   Godot_v4.7.2.2_mono_win64.exe --path Game --editor res://main.tscn
   ```
3. With the editor still running, touch `Game/GameScript.cs` (or add a blank
   line to it) and rebuild from a terminal:
   ```
   dotnet build Game/Game.csproj
   ```
   The editor notices the new `Game.dll` and reloads the assembly.

### Expected (broken) output

The editor's output (stderr) prints one error per affected library class, e.g.:

```
ERROR: Script class can only be set together with base class name
   at: Array::set_typed (core\variant\array.cpp:892)
   C# backtrace (most recent call first):
       [0] void Godot.Collections.Array`1.SetTypedForUnderlyingArray() (...)
       [1] Godot.Collections.Array`1..ctor() (...)
       [2] ... TypedArrayBug.Lib.TaskPrint.GetGodotPropertyDefaultValues() (...)
       ...
       [13] void Godot.Bridge.ScriptManagerBridge.LookupScriptsInReferencedAssemblies(...) (.../ScriptManagerBridge.cs:398)
```

The array falls back to being untyped. Nothing crashes, matching the reports in
godotengine/godot#103343.

With the fix, the reload produces no such error.

## Notes

* The error only appears after a **reload**, not on a cold start: on the first
  load the script bridges are created in order, while a reload clears the native
  script type info and then loads the referenced assembly's scripts one by one
  (`LookupScriptsInReferencedAssemblies`).
* The versions in `Game.csproj` and `Libs/TypedArrayBug.Lib/TypedArrayBug.Lib.csproj`
  must match the fork editor release under test. Change both if needed.
* `GameScript.cs` does not need to do anything; it only has to exist in the game
  assembly so editing it makes the loaded assembly out of date.
