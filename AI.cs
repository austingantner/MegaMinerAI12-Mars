using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using CSClient;

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
class AI : BaseAI
{
    // Enum for types of units you can spawn.
    public enum Types { Worker, Scout, Tank };

    public override string username()
    {
        return "SwineFlew";
    }

    public override string password()
    {
        return "password";
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        Console.WriteLine("starting turn");
        ourRun();
        Console.WriteLine("ending turn");
        return true;
    }

    public bool shellAI()
    {

        int numberOfUnits = 0;

        // Get the number of units owned.
        for (int i = 0; i < units.Length; i++)
            if (units[i].Owner == playerID())
                numberOfUnits++;

        // Look for tiles I own.
        for (int i = 0; i < tiles.Length; i++)
        {
            // If this tile is my spawn tile or my pump station...
            if (tiles[i].Owner == playerID())
            {
                // Get the unit cost for a worker.
                int cost = Int32.MaxValue;
                for (int j = 0; j < unitTypes.Length; j++)
                    if (unitTypes[j].Type == (int)Types.Worker)
                        cost = unitTypes[j].Cost;

                // If there is enough oxygen to spawn the unit...
                if (players[playerID()].Oxygen >= cost)
                {
                    // ...and if we can spawn more units...
                    if (numberOfUnits < maxUnits())
                    {
                        // ...and nothing is spwning on the tile...
                        if (!tiles[i].IsSpawning)
                        {
                            bool canSpawn = true;

                            // If it's a pump station and it's not being seiged...
                            if (tiles[i].PumpID != -1)
                            {
                                // ...find the pump in the vector.
                                for (int j = 0; j < pumpStations.Length; j++)
                                {
                                    // If it's being sieged, don't spawn.
                                    if (pumpStations[j].Id == tiles[i].PumpID && pumpStations[j].SiegeAmount > 0)
                                        canSpawn = false;
                                }
                            }

                            // If there is someone else on the tile, don't spawn.
                            for (int j = 0; j < units.Length; j++)
                                if (tiles[i].X == units[j].X && tiles[i].Y == units[j].Y)
                                    canSpawn = false;

                            // If possible, spawn!
                            if (canSpawn)
                            {
                                tiles[i].spawn((int)Types.Worker);
                                numberOfUnits++;
                            }
                        }
                    }
                }
            }
        }

        int moveDelta = 0;

        // Set to move left or right based on ID; towards the center.
        moveDelta = playerID() == 0 ? 1 : -1;

        // Do some stuff for each unit.
        for (int i = 0; i < units.Length; i++)
        {
            // If you don't own the unit, ignore it.
            if (units[i].Owner != playerID())
                continue;

            // Try to move to the right or left movement times.
            for (int z = 0; z < units[i].MaxMovement; z++)
            {
                bool canMove = true;

                // If there's a unit there, don't move.
                for (int j = 0; j < units.Length; j++)
                {
                    if (units[i].X + moveDelta == units[j].X && units[i].Y == units[j].Y)
                        canMove = false;
                }

                // If nothing is there, and it's not moving off the edge of the map...
                if (canMove && units[i].X + moveDelta >= 0 && units[i].X + moveDelta < mapWidth())
                {
                    // If the tile is not an enemy spawn point...
                    if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].PumpID == -1 &&
                      tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 1 - playerID()) ||
                      tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 2)
                    {
                        // If the tile is not an ice tile...
                        if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].Owner == 3 &&
                          tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].WaterAmount > 0))
                        {
                            // If the tile is not spawning anything...
                            if (!(tiles[(units[i].X + moveDelta) * mapHeight() + units[i].Y].IsSpawning))
                            {
                                // If the unit is alive...
                                if (units[i].HealthLeft > 0)
                                {
                                    // Move the unit!
                                    units[i].move(units[i].X + moveDelta, units[i].Y);
                                }
                            }
                        }
                    }
                }
            }

            // If there's an enemy in the movement direction and the unit hasn't attacked and is alive.
            if (!units[i].HasAttacked && units[i].HealthLeft > 0)
            {
                for (int j = 0; j < units.Length; j++)
                {
                    // Check if there is a enemy unit in the direction.
                    if (units[i].X + moveDelta == units[j].X && units[i].Y == units[j].Y &&
                      units[j].Owner != playerID())
                    {
                        // Attack it!
                        units[i].attack(units[j]);
                        break;
                    }
                }
            }

            // If there's a space to dig below the unit and the unit hasn't dug, and the unit is alive.
            if (units[i].Y != mapHeight() - 1 &&
              tiles[units[i].X * mapHeight() + units[i].Y + 1].PumpID == -1 &&
              tiles[units[i].X * mapHeight() + units[i].Y + 1].Owner == 2 &&
              units[i].HasDug == false &&
              units[i].HealthLeft > 0)
            {
                bool canDig = true;

                // Make sure there's no unit on that tile.
                for (int j = 0; j < units.Length; j++)
                    if (units[i].X == units[j].X && units[i].Y + 1 == units[j].Y)
                        canDig = false;

                // Make sure the tile is not an ice tile.
                if (canDig && !(tiles[units[i].X * mapHeight() + units[i].Y + 1].Owner == 3 &&
                  tiles[units[i].X * mapHeight() + units[i].Y + 1].WaterAmount > 0))
                {
                    units[i].dig(tiles[units[i].X * mapHeight() + units[i].Y + 1]);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// This function is called once, before your first turn.
    /// </summary>
    public override void init()
    {
        BitBoard.Initialize(this);
    }

    /// <summary>
    /// This function is called once, after your last turn.
    /// </summary>
    public override void end() { }

    public AI(IntPtr c) : base(c) { }

    //our code =================================================

    public static bool phase2;
    public static HashSet<int> defenders = new HashSet<int>();
    public static List<int> xSpawn = new List<int>();
    public static List<int> ySpawn = new List<int>();

    public void ourRun()
    {
  System.Console.WriteLine("Turn number " + turnNumber());
        while (xSpawn.Count > 0)
        {
            foreach (Unit u in units)
            {
                if (u.Owner == playerID())
                    if (u.X == xSpawn[0] && u.Y == ySpawn[0])
                    {
                        defenders.Add(u.Id);
                        break;
                    }
            }
            xSpawn.RemoveAt(0);
            ySpawn.RemoveAt(0);
        }

      
        BitBoard.UpdateAll();
        if (players[playerID()].WaterStored <= players[Math.Abs(playerID() - 1)].WaterStored && BitBoard.Equal(BitBoard.empty, new BitArray(BitBoard.length).Or(BitBoard.myConnectedPumpStations).Or(BitBoard.oppConnectedPumpStations)))
        {
            phase2 = true;
            Console.WriteLine("we are loosing");
        }
        betterSpawn();
        BitBoard.UpdateAll();
        CIA.executeMissions(assignMissions());
    }

    public void spawnUnits()
    {
        int numberOfUnits = 0;

        // Get the number of units owned.
        for (int i = 0; i < units.Length; i++)
            if (units[i].Owner == playerID())
                numberOfUnits++;
        if (1 == playerID())
            for (int i = 0; i < tiles.Length; i++)
                innerloop(i);
        else
            for (int i = tiles.Length - 1; i >= 0; i--)
                innerloop(i);
    }

    private void innerloop(int i)
    {
        // If this tile is my spawn tile or my pump station...
        if (tiles[i].Owner == playerID())
        {
            int cost = Int32.MaxValue;
            for (int j = 0; j < unitTypes.Length; j++)

                if (turnNumber() <= 1 && unitTypes[j].Type == (int)Types.Worker)
                    cost = unitTypes[j].Cost;
                else if (turnNumber() > 1 && unitTypes[j].Type == (int)Types.Scout)
                    cost = unitTypes[j].Cost;

            // If there is enough oxygen to spawn the unit...
            if (players[playerID()].Oxygen >= cost)
            {
                // ...and nothing is spwning on the tile...
                if (!tiles[i].IsSpawning)
                {
                    bool canSpawn = true;

                    // If it's a pump station and it's not being seiged...
                    if (tiles[i].PumpID != -1)
                    {
                        // ...find the pump in the vector.
                        for (int j = 0; j < pumpStations.Length; j++)
                        {
                            // If it's being sieged, don't spawn.
                            if (pumpStations[j].Id == tiles[i].PumpID && pumpStations[j].SiegeAmount > 0)
                                canSpawn = false;
                        }
                    }

                    // If there is someone else on the tile, don't spawn.
                    for (int j = 0; j < units.Length; j++)
                        if (tiles[i].X == units[j].X && tiles[i].Y == units[j].Y)
                            canSpawn = false;
                    bool defender = false;
                    if (turnNumber() > 1 && BitBoard.GetBit(BitBoard.myConnectedPumpStations, tiles[i].X, tiles[i].Y))
                    {
                        defender = true;
                    }
                    foreach (Unit u in units)
                    {
                        if (defenders.Contains(u.Id)
                            && BitBoard.GetBit(BitBoard.myConnectedPumpStations, tiles[i].X, tiles[i].Y)
                            && BitBoard.GetBit(new BitArray(BitBoard.length, false).Or
                            (BitBoard.myConnectedPumpStations).Or
                            (BitBoard.GetAdjacency(BitBoard.myConnectedPumpStations)), u.X, u.Y)
                           )
                        {
                            canSpawn = false;
                        }
                    }

                    // If possible, spawn!
                    if (canSpawn)
                    {
                        if (defender)
                        {
                            tiles[i].spawn((int)Types.Tank);
                            xSpawn.Add(tiles[i].X);
                            ySpawn.Add(tiles[i].Y);
                        }
                        if (turnNumber() <= 1)
                            tiles[i].spawn((int)Types.Worker);
                        else
                            tiles[i].spawn((int)Types.Scout);
                    }
                }
            }
        }
    }

    public void betterSpawn()
    {
        if (turnNumber() <= 1)
        {
            HashSet<int> pumpid = new HashSet<int>();
            List<int> pumpStationIndexes = BitBoard.GetIndexes(BitBoard.myConnectedPumpStations);

            foreach (Tile t in tiles)
            {
                if (t.Owner != playerID()) continue;
                if (t.PumpID == -1) continue;
                if (pumpid.Contains(t.PumpID)) continue;
                if (t.IsSpawning) continue;
                bool temp = true;
                foreach (int i in pumpStationIndexes)
                {
                    if (BitBoard.GetX(i) == t.X && BitBoard.GetY(i) == t.Y)
                    {
                        temp = false;
                        break;
                    }
                }
                if (temp) continue;
                temp = false;
                foreach (Unit u in units)
                {
                    if (u.X == t.X && u.Y == t.Y)
                    {
                        temp = true;
                        break;
                    }
                }
                if (temp) continue;


                pumpid.Add(t.PumpID);
                t.spawn((int)Types.Tank);
                xSpawn.Add(t.X);
                ySpawn.Add(t.Y);
            }
        }
        spawnUnits();
    }

    public List<Mission> assignMissions()
    {
        List<Mission> offensivemissions = new List<Mission>();
        List<Mission> defensivemissions = new List<Mission>();
        List<Mission> attackmissions = new List<Mission>();
        bool first = true;
        foreach (Unit u in units)
        {
            if (u.Owner == playerID())
            {
                if (defenders.Contains(u.Id))
                {
                    defensivemissions.Add(new Mission(u, /*() => BitBoard.GetPumpStation(BitBoard.myConnectedPumpStations, u.X, u.Y)*/
                                                    () => BitBoard.myConnectedPumpStations, Mission.missionTypes.defendAndTrench));
                    //missions.Add(new Mission(u,
                    //    () => BitBoard.GetPumpStation(new BitArray(BitBoard.length, false).Or(BitBoard.myConnectedPumpStations).Or(BitBoard.oppConnectedPumpStations),
                    //    u.X, u.Y), Mission.missionTypes.defendPumpStation));
                }
                else if (u.Type == (int)Types.Scout || u.Type == (int)Types.Worker)
                {
                    if (!BitBoard.Equal(BitBoard.oppConnectedPumpStations, BitBoard.empty))
                    {
                        if (first)
                        {
                            first = false;
                            foreach (Unit e in units)
                            {
                                if (e.Owner != u.Owner && e.Type == (int)Types.Tank)
                                {
                                    offensivemissions.Add(new Mission(u, () => BitBoard.position[e.X][e.Y], Mission.missionTypes.goAttack));
                                }
                            }
                        }
                        offensivemissions.Add(new Mission(u, () => BitBoard.oppConnectedPumpStations, Mission.missionTypes.goAttack));
                        offensivemissions.Add(new Mission(u, () => BitBoard.oppConnectedPumpStations, Mission.missionTypes.goAttack, true));
                    }
                    else
                    {
                        offensivemissions.Add(new Mission(u, () => BitBoard.oppOccupiedTiles, Mission.missionTypes.goAttack));
                    }
                }
                attackmissions.Add(new Mission(u, () => BitBoard.empty, Mission.missionTypes.attackInRange));
            }
        }
        offensivemissions.AddRange(defensivemissions);
        offensivemissions.AddRange(attackmissions);
        return offensivemissions;
    }
}
