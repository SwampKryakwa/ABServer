using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class MirrorFlash : AbilityCard
    {
        public MirrorFlash(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position && x.InBattle && x.IsEnemyOf(User) && x.Power > User.Power}
            ];
        }

        public override void TriggerEffect()
        {
            Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

            short difference = (short)(User.Power - target.Power);
            new BoostMultipleVariousEffect(User, [User, target], [(short)-difference, difference], TypeId, (int)Kind).Activate();
            new BoostEffect(User, User, -100, TypeId, (int)Kind).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Lumina) && user.Position.Bakugans.Any(user.IsEnemyOf);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => user.IsEnemyOf(x) && x.Power > user.Power);
    }
}
