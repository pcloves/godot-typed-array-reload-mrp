using Godot;

namespace TypedArrayBug.Lib;

// A Node script from the external assembly, kept alive by opening main.tscn so
// a reload has an existing instance of it to re-create.
[Tool]
[GlobalClass]
public partial class TaskRoot : Node
{
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [];

    [Export]
    public Resource? Sample { get; set; }
}
