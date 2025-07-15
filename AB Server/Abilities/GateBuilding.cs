using AB_Server.Gates;
using AB_Server.Gates.SpecialGates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class GateBuilding : AbilityCard
    {
        public GateBuilding(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new MultiGateSlotSelector { Message = "INFO_ABILITY_TARGET", ForPlayer = x => x == Owner, MaxNumber = 2 }
            ];
        }

        public override void TriggerEffect()
        {
            var slots = (CondTargetSelectors[0] as MultiGateSlotSelector)!.SelectedSlots;

            List<GateOfSubterra80> cards = [];
            List<(byte, byte)> positions = [];
            List<byte> players = [];
            
            foreach ((int X, int Y) in slots)
            {
                var gate = new GateOfSubterra80(Game.GateIndex.Count, Owner);
                Game.GateIndex.Add(gate);
                cards.Add(gate);
                positions.Add(((byte)X, (byte)Y));
                players.Add(Owner.Id);
            }

            GateCard.MultiSet(Game, [.. cards], [.. positions], [.. players]);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Subterra) && user.OnField();
    }
}
