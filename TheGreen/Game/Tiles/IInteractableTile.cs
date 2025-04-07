using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Tiles
{
    internal interface IInteractableTile
    {
        void OnRightClick(int x, int y);
    }
}
