﻿using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class VicariousVictim : AbilityCard
    {
        public VicariousVictim(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InDrop()}
            ];
        }

        public override void TriggerEffect()
        {
            var selectedBakugan = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.Position is GateCard positionGate)
                if (selectedBakugan.Position is BakuganDrop)
                {
                    selectedBakugan.MoveFromDropToField(positionGate);
                    User.MoveFromFieldToDrop(positionGate.EnterOrder);
                }
                else if (selectedBakugan.Position is GateCard targetPositionGate)
                {
                    selectedBakugan.Move(positionGate, new JObject { ["MoveEffect"] = "Slide" });
                    User.Move(targetPositionGate, new JObject { ["MoveEffect"] = "Slide" });
                }
                else if (selectedBakugan.Position is Player)
                {
                    selectedBakugan.AddFromHand(positionGate);
                    User.MoveFromFieldToHand(positionGate.EnterOrder);
                }
        }

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && Owner.BakuganDrop.Bakugans.Count != 0 && user.Type == BakuganType.Griffon;
    }
}