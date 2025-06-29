using System;
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

        public override bool IsOpenable() => false;

        public override void CheckAutoBattleStart()
        {
            int[] teamsCount = new int[game.TeamCount];
            for (int i = 0; i < game.TeamCount; i++)
                teamsCount[i] = 0;

            foreach (var bakugan in Bakugans)
                teamsCount[bakugan.Owner.TeamId]++;

            if (OpenBlocking.Count == 0 && !IsOpen && !Negated && teamsCount.Any(x => x >= 2))
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated)
                Bakugans.ForEach(x => x.DestroyOnField(EnterOrder));

            game.ChainStep();
        }
    }
}
