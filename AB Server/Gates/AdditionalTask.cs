using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class AdditionalTask : GateCard
    {
        public AdditionalTask(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 11;

        public override bool IsOpenable() => false;

        public override void Set(byte posX, byte posY)
        {
            game.BakuganAdded += CheckAutoConditions;
            base.Set(posX, posY);
        }

        public override void Dispose()
        {
            game.BakuganAdded -= CheckAutoConditions;
            base.Dispose();
        }

        public override void CheckAutoConditions(Bakugan target, byte owner, IBakuganContainer pos)
        {
            if (pos != this || IsOpen || Negated) return;

            if (Bakugans.Count >= 2)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, EnterOrder[^1])
            ));

            game.OnAnswer[Owner.Id] = Setup1;
        }

        Bakugan target;

        public void Setup1()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.NextStep();
        }

        public override void Resolve()
        {
            if (!Negated && target.Position == this)
                target.ToHand(EnterOrder);
        }
    }
}
