using Godot;

namespace TypedArrayBug.Lib;

// Each concrete subclass exports a collection whose element type is the shared
// base class, and initializes it inline.
[Tool]
[GlobalClass]
public partial class TaskRunAll : TaskBase
{
    // Non-empty initializer: pulls TaskPrint into the cold-start load set, so its
    // script stays bridged and a reload leaves it invalidated. This type is live in
    // the scene, so a reload does not reconstruct it; TaskRunSerial carries the same
    // initializer shape into the reload window.
    [Export]
    public Godot.Collections.Array<TaskBase> Steps { get; set; } = [new TaskPrint()];
}
