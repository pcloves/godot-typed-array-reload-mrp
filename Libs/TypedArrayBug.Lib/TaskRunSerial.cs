using Godot;

namespace TypedArrayBug.Lib;

[Tool]
[GlobalClass]
public partial class TaskRunSerial : TaskBase
{
    // Non-empty initializer like TaskRunAll.Steps. This type is never reached on a
    // cold start, so this constructor only runs during the reload window — when the
    // element type (TaskPrint, kept alive by the scene) is still invalidated on an
    // engine that only has the first commit of matelq/godot#2.
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [new TaskPrint()];
}
