﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSClient
{
    class CIA
    {
        public static void executeMissions(List<Mission> missions)
        {
            foreach (Mission m in missions)
            {
                BitArray target = m.target();
                if (!target.Equals(BitBoard.empty))
                {
                    switch (m.missionType)
                    {
                        case Mission.missionTypes.goTo:
                            missionGoTo(m.agent, target, m.walkThroughWater);
                            break;
                    }
                }
            }
        }

        
        //goto should go to the nearest non-occupied square
        //from the target bit board
        private static void missionGoTo(Unit u, BitArray b, bool walkThroughWater)
        {
            if (u.MovementLeft == 0)
            {
                return;
            }

						List<Node> path = AStar.route(u.X, u.Y, b, !walkThroughWater);
						foreach(Node n in path)
						{
							if(u.MovementLeft == 0) break;

							u.move(n.x, n.y);
						}

            BitBoard.Update();
        }
    }
}
