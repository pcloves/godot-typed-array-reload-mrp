using Godot;

namespace TypedArrayBug.Lib;

[Tool]
[GlobalClass]
public partial class TaskRunAny : TaskBase
{
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [];
}
