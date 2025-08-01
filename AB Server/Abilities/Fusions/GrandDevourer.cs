﻿using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities.Fusions
{
    internal class GrandDevourer : FusionAbility
    {
        public GrandDevourer(int cID, Player owner) : base(cID, owner, 12, typeof(Earthmover))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTROYTARGET", TargetValidator = x => x.OnField() }
            ];

            ResTargetSelectors =
            [
                new YesNoSelector() { ForPlayer = x => x == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner, Message = "INFO_ABILITY_WANTDISCARD", Condition = () =>(CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner.AbilityHand.Count != 0, IsYes = false },
                new AbilitySelector() { ClientType = "A", Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes, ForPlayer = x => x == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner, Message = "INFO_ABILITY_DISCARDTARGET", TargetValidator = x => x.Owner == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner }
            ];
        }

        public override void TriggerEffect()
        {
            if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes)
            {
                (ResTargetSelectors[1] as AbilitySelector)!.SelectedAbility.Discard();
            }
            else
            {
                var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
                if (target.Position is GateCard posGate)
                    target.MoveFromFieldToDrop(posGate.EnterOrder);
                else if (target.InHand())
                    target.MoveFromHandToDrop();
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Worm && user.OnField();
    }
}
