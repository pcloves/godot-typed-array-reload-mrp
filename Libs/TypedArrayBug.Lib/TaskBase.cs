using Godot;

namespace TypedArrayBug.Lib;

// Abstract [GlobalClass] base class. It is never instantiated directly; it only
// serves as the element type of the exported collections in the subclasses.
[Tool]
[GlobalClass]
public abstract partial class TaskBase : Resource
{
}
