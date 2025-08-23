using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class MirrorFlash : AbilityCard
    {
        public MirrorFlash(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.InBattle && x.IsOpponentOf(User) && x.Power > User.Power}
            ];
        }

        public override void TriggerEffect()
        {
            Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            short difference = (short)(User.Power - target.Power);
            User.Boost(-difference, this);
            target.Boost(difference, this);
            User.Boost(-100, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Lumina) && user.Position.Bakugans.Any(user.IsOpponentOf);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => user.IsOpponentOf(x) && x.Power > user.Power);

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(27, CardKind.NormalAbility, (cID, owner) => new MirrorFlash(cID, owner, 27));
    }
}
