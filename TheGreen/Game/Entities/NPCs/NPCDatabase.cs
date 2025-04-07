using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TheGreen.Game.Entities.NPCs.Behaviors;

namespace TheGreen.Game.Entities.NPCs
{
    public static class NPCDatabase
    {

        private static Dictionary<int, NPC> _npcs = new Dictionary<int, NPC>
        {
            {0, new NPC(0, "Mutant Cricket", ContentLoader.EnemyTextures[0], new Vector2(69, 34), 100, 10, true, new MutantCricketBehavior(), animationFrames: new List<(int, int)> { (0, 3), (4, 4)})}
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns>A new npc instance with the specified id</returns>
        public static NPC InstantiateNPCByID(int npcID)
        {
            return NPC.CloneNPC(_npcs[npcID]);
        }
    }
}
