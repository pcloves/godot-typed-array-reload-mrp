using Godot;

namespace TypedArrayBugMRP;

// Nothing depends on this script. It exists so the game project has a script of
// its own: rebuilding the game assembly is what makes the editor notice the
// change and reload the assembly.
public partial class GameScript : Node
{
}
