using Godot;

namespace TypedArrayBug.Lib;

[Tool]
[GlobalClass]
public partial class ConditionRandomSelect : ConditionBase
{
    [Export]
    public Godot.Collections.Array<ConditionBase> Conditions { get; set; } = [];
}
