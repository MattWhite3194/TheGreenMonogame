
namespace TheGreen.Game.Entities.NPCs.Behaviors
{
    /// <summary>
    /// Interface defining the methods required for an enemy movement pattern
    /// </summary>
    public interface INPCBehavior
    {
        void AI(double delta, NPC enemy);
        INPCBehavior Clone();
    }
}
