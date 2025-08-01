﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class MindGhost : GateCard
    {
        public MindGhost(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 18;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Intermediate && BattleStarting && AtLeastTwoBakuganFromSameTeam() && OpenBlocking.Count == 0 && !IsOpen && !Negated;

        bool AtLeastTwoBakuganFromSameTeam()
        {
            int[] teamsCount = new int[game.TeamCount];
            for (int i = 0; i < game.TeamCount; i++)
                teamsCount[i] = 0;

            foreach (var bakugan in Bakugans)
                teamsCount[bakugan.Owner.TeamId]++;

            return teamsCount.Any(x => x >= 2);
        }

        public override void Resolve()
        {
            if (!Negated)
                new List<Bakugan>(Bakugans).ForEach(x => x.MoveFromFieldToDrop(EnterOrder));

            game.ChainStep();
        }
    }
}
