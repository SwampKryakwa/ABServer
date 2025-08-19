using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class LightningShield : AbilityCard
    {
        public LightningShield(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.InBattle && x.IsOpponentOf(User)}
            ];
        }

        public override void TriggerEffect()
        {
            if (CondTargetSelectors[0] is BakuganSelector targetSelector)
            {
                if (!targetSelector.SelectedBakugan.IsAttribute(Attribute.Lumina))
                    targetSelector.SelectedBakugan.Boost(-100, this);
                if (targetSelector.SelectedBakugan.Power > User.Power)
                    targetSelector.SelectedBakugan.Boost(-100, this);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Lumina) && user.Position.Bakugans.Any(user.IsOpponentOf);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(user.IsOpponentOf);

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(26, CardKind.NormalAbility, (cID, owner) => new LightningShield(cID, owner, 26));
    }
}
