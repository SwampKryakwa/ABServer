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

        private Bakugan target;

        public override void Open()
        {
            // Target 1 bakugan on this card
            var candidates = Bakugans.ToArray();
            if (candidates.Length == 0)
            {
                game.ChainStep();
                return;
            }

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_POWERSPIKE_SELECT", TypeId, (int)Kind, candidates)
            ));

            game.OnAnswer[Owner.Id] = () =>
            {
                target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
                game.CheckChain(Owner, this);
            };
        }

        // Precompute the steps from -300 to +300 in increments of 50
        int[] steps = Enumerable.Range(-6, 7).Select(i => i * 50).ToArray();
        public override void Resolve()
        {
            if (Negated || target == null || !target.OnField() || target.Position != this)
            {
                game.ChainStep();
                return;
            }

            // Randomly pick -300 to +300 (in steps of 50)
            int boost = steps[new Random().Next(steps.Length)];

            target.Boost(new Boost((short)boost), this);

            game.ChainStep();
        }
    }
}
