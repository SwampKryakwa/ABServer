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

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Intermediate && BattleOver && OpenBlocking.Count == 0 && !IsOpen && !Negated;

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
