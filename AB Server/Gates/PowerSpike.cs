using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class PowerSpike : GateCard
    {
        public PowerSpike(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 17;

        public override void Resolve()
        {
            if (!Negated)
            {
                game.ThrowEvent(Owner.Id, new JObject
                {
                    ["Type"] = "StartSelection",
                    ["Selections"] = new JArray {
                        EventBuilder.FieldBakuganSelection("INFO_GATE_BOOSTTARGET", TypeId, (int)Kind, Bakugans)
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

            target.Boost(new Boost((short)(50 * new Random().Next(-6, 7))), this);

            game.ChainStep();
        }
    }
}
