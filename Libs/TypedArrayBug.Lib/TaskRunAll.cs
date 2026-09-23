using Godot;

namespace TypedArrayBug.Lib;

// Each concrete subclass exports a collection whose element type is the shared
// base class, and initializes it inline.
[Tool]
[GlobalClass]
public partial class TaskRunAll : TaskBase
{
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [];
}
