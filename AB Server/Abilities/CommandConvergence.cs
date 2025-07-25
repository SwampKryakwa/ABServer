﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class CommandConvergence : AbilityCard
    {
        public CommandConvergence(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new OptionSelector() { Message = "INFO_PICKER_ATTRIBUTE", ForPlayer = (p) => p == Owner, OptionCount = 6 }
            ];
        }

        public override void TriggerEffect()
        {
            Bakugan[] targets = [.. Game.BakuganIndex.Where(x => x.OnField() && x.IsAttribute((Attribute)(ResTargetSelectors[0] as OptionSelector)!.SelectedOption))];
            foreach (Bakugan target in targets)
                target.Boost(100, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Knight && user.OnField();
    }
}
