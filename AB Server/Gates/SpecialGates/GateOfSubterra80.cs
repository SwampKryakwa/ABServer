using AB_Server.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates.SpecialGates
{
    internal class GateOfSubterra80 : GateCard
    {
        public GateOfSubterra80(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 1;
        public override CardKind Kind { get; } = CardKind.SpecialGate;

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
            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, 4, Bakugans.Where(x => x.Owner == Owner))
            ));

            foreach (var bakugan in Bakugans.Where(x => x.IsAttribute(Attribute.Subterra)))
            {
                bakugan.Boost(new Boost(80), this);
            }

            game.ChainStep();
        }
    }
}