using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class Anastasis : GateCard
    {
        public Anastasis(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 19;

        public override bool IsOpenable() => false;

        public override void CheckAutoBattleEnd()
        {
            if (OpenBlocking.Count == 0 && !IsOpen && !Negated)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated)
            {
                game.ThrowEvent(Owner.Id, new JObject
                {
                    ["Type"] = "StartSelection",
                    ["Selections"] = new JArray {
                        EventBuilder.FieldBakuganSelection("INFO_GATE_BOOSTTARGET", TypeId, (int)Kind, bakugansDefeatedThisBattle)
                    }
                });

                game.OnAnswer[Owner.Id] = Activate;
            }
            else
                game.ChainStep();
        }

        public void Activate()
        {
            Bakugan target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

            target.MoveFromDropToHand();

            game.ChainStep();
        }
    }
}
