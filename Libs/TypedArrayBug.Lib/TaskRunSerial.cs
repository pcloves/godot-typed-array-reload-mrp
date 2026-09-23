using Godot;

namespace TypedArrayBug.Lib;

[Tool]
[GlobalClass]
public partial class TaskRunSerial : TaskBase
{
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [];
}
