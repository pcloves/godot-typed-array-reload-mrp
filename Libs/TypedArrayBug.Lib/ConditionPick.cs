using Godot;

namespace TypedArrayBug.Lib;

// Minimal concrete ConditionBase used as an element of ConditionRandomSelect's
// collection. It exists so that collection can have a non-empty inline
// initializer without self-reference (which would recurse infinitely).
[Tool]
[GlobalClass]
public partial class ConditionPick : ConditionBase
{
}
