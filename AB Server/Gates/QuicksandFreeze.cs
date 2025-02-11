using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    class QuicksandFreeze : GateCard
    {
        public override void DetermineWinner()
        {
            if (!Negated)
                Open();
            else
                base.DetermineWinner();
        }

        public override void Open()
        {
            IsOpen = true;
            resolved = false;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.AwaitingAnswers[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            foreach (Bakugan b in Bakugans)
            {
                b.JustEndedBattle = true;
            }
            ActiveBattle = false;

            var numSides = Bakugans.Select(x => x.Owner.SideID).Distinct().Count();
            BattleOver = true;

            if (Bakugans.Count == 1) return;
            if (numSides > 1) DetermineWinnerNormalBattle();
            else if (numSides == 1) DetermineWinnerFakeBattle();
        }

        bool resolved = false;

        public override void Dispose()
        {
            if (resolved || !IsOpen)
                base.Dispose();
            else
            {
                if (!CheckBattles())
                {
                    foreach (Bakugan b in new List<Bakugan>(Bakugans))
                    {
                        b.JustEndedBattle = false;
                        if (b == target) continue;
                        b.ToHand(EnterOrder);
                    }
                }
                else game.ContinueGame();
            }
            resolved = true;
        }

        public override bool IsOpenable() =>
            false;
    }
}
