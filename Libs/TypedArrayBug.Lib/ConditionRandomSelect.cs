using Godot;

namespace TypedArrayBug.Lib;

[Tool]
[GlobalClass]
public partial class ConditionRandomSelect : ConditionBase
{
    // Non-empty initializer (same shape as TaskRunAll.Steps): covers the second
    // collection type. The element must be a different class so the initializer
    // does not recurse into itself.
    [Export]
    public Godot.Collections.Array<ConditionBase> Conditions { get; set; } = [new ConditionPick()];
}
