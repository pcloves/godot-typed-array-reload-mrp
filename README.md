# MRP: typed collections during a referenced-assembly reload

Minimal reproduction for two failures that appear in the same C# assembly-reload
window with multi-assembly support:

- **Symptom 1** – after a reload, every exported typed collection whose element
  type is a script class from a **referenced assembly** prints
  `Script class can only be set together with base class name`, and the array
  silently falls back to being untyped.
- **Symptom 2** – with symptom 1 fixed, the same window starts dropping the
  elements of such collections:
  `Attempted to push_back an object into a TypedArray, that does not inherit from 'CSharpScript'.`

Reported in [matelq/godot#1](https://github.com/matelq/godot/issues/1); both
symptoms are fixed by
[matelq/godot#2](https://github.com/matelq/godot/pull/2). This repository only
contains the reproduction; the fixes live in the fork.

## Layout

| Path | What it is |
|---|---|
| `Game/` | Godot project. `project.godot` + `Game.csproj` (CrackTower SDK 4.7.2.2). |
| `Game/GameScript.cs` | A script in the game assembly. Editing/rebuilding it is what makes the editor hot-reload. |
| `Game/main.tscn` | A `Node` using `TaskRoot` from the library, with a `main_task.tres` instance assigned. Opening it keeps live instances of library types around, so the reload has to re-create them. |
| `Game/main_task.tres` | A `TaskRunAll` `Resource`. Keeping it live is what pulls `TaskPrint` into the cold-start load set. |
| `Libs/TypedArrayBug.Lib/` | Referenced assembly whose sources live outside the Godot project directory, so its classes get `csharp://` script paths. |

The library mirrors the shape that triggers the bug:

* `TaskBase` / `ConditionBase` – abstract `[GlobalClass]` resources, used as the
  element types of the exported collections.
* `TaskRunAll` / `TaskRunSerial` – export `Godot.Collections.Array<TaskBase>`
  initialized inline with one element (`= [new TaskPrint()]`). `TaskRunAll` is
  live from a cold start (it is the class of `main_task.tres`); `TaskRunSerial`
  is only ever reached by the reload pass, which is the case that drops the
  element on an engine that has the first commit only.
* `TaskRunAny` / `TaskPrint` – the same export with an empty initializer (`= []`).
* `ConditionRandomSelect` – `Godot.Collections.Array<ConditionBase>` with
  `= [new ConditionPick()]` (the second constraint type; its scripts are loaded
  fresh in the reload window, so it never fails).
* `ConditionPick` – minimal concrete `ConditionBase` used as that element.
* `TaskRoot` – a `Node` script from the library so the opened scene has an
  instance of a library type.

## Reproduce

The committed configuration targets the released **4.7.2.2** fork editor with the
`CrackTower.Godot.*` packages from nuget.org (see Notes for the patched engines).

1. Build the solution once so the editor finds an up-to-date assembly:
   ```
   dotnet build Game/Game.csproj
   ```
2. Open the project in the editor and open `main.tscn` in it:
   ```
   <editor.exe> --path Game --editor res://main.tscn
   ```
3. With the editor still running, touch `Game/GameScript.cs` (or add a blank
   line to it) and rebuild from a terminal:
   ```
   dotnet build Game/Game.csproj
   ```
   The editor notices the new `Game.dll` and reloads the assembly.

### What the reload prints, by engine state

The two error patterns are counted separately:

| Engine | Reload happened | `Script class can only be set...` | `Attempted to push_back...` |
|---|---|---|---|
| 4.7.2.2 release (`f8718bd17`, unpatched) | yes | 3 | 0 |
| First commit only (`d63d88709d`) | yes | 0 | 1 |
| Both commits (PR head `065d0090c7`) | yes | 0 | 0 |

Counts can vary slightly with script load order; the pattern (which column is
non-zero) is what matters.

Symptom 1, one error per affected class:

```
ERROR: Script class can only be set together with base class name
   at: Array::set_typed (core\variant\array.cpp:892)
   C# backtrace (most recent call first):
       [0] void Godot.Collections.Array`1.SetTypedForUnderlyingArray() (...)
       [1] Godot.Collections.Array`1..ctor() (...)
       [2] ... TypedArrayBug.Lib.TaskRunSerial.GetGodotPropertyDefaultValues() (...)
       ...
       [13] ... ScriptManagerBridge.LookupScriptsInReferencedAssemblies(...) (.../ScriptManagerBridge.cs:398)
```

Nothing crashes, matching the reports in
[godotengine/godot#103343](https://github.com/godotengine/godot/issues/103343).

Symptom 2, on the first-commit-only engine: the reload pass constructs
`TaskRunSerial`, and the `TaskPrint` element it pushes is still invalidated, so
the element is dropped:

```
ERROR: Attempted to push_back an object into a TypedArray, that does not inherit from 'CSharpScript'.
   at: ContainerTypeValidate::_internal_validate_object (core\variant\container_type_validate.h:137)
   C# backtrace (most recent call first):
       [0] void Godot.Collections.Array`1.Add(T) (...)
       [1] ... TypedArrayBug.Lib.TaskRunSerial.GetGodotPropertyDefaultValues() (...)
       ...
       [12] ... NativeFuncs.godotsharp_internal_script_load (...)
       [13] ... ScriptManagerBridge.LookupScriptsInReferencedAssemblies(...) (.../ScriptManagerBridge.cs:398)
```

With both commits of the PR, the reload produces no such error.

## Notes

* Both errors only appear after a **reload**, not on a cold start: a reload
  clears the native script type info and then loads the referenced assembly's
  scripts one by one (`LookupScriptsInReferencedAssemblies`). During that window
  a `csharp://` script fetched from the resource cache is still invalid. The
  class-name layer fails first (symptom 1, the array ends up untyped) and, once
  the collection stays correctly typed, the element check fails against the
  still-invalid script and the element is dropped (symptom 2). The follow-up
  commit revives such scripts in `godotsharp_internal_script_load`.
* The versions in `Game.csproj` and `Libs/TypedArrayBug.Lib/TypedArrayBug.Lib.csproj`
  must match the editor under test. To test a patched engine, build the fork at
  the commit under test — the build pushes matching `4.7.2.3` packages to the
  local NuGet source — and switch `Game.csproj` to `4.7.2.3`; the library
  project can stay on the `4.7.2.2` packages.
* `GameScript.cs` does not need to do anything; it only has to exist in the game
  assembly so editing it makes the loaded assembly out of date.
