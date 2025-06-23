using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class BlowAway : AbilityCard
    {
        public BlowAway(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.IsEnemyOf(User) && target.Position == User.Position}
            ];
        }

        public override void TriggerEffect()
        {
            GateCard[] possibleDestinations = Game.GateIndex.Where(x => x != User.Position && x.OnField).ToArray();
            new MoveBakuganEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, possibleDestinations[new Random().Next(possibleDestinations.Length)], TypeId, (int)Kind).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.IsAttribute(Attribute.Zephyros) && Game.GateIndex.Count(x => x.OnField) >= 2 && Game.CurrentWindow == ActivationWindow.Normal;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => x.Owner != user.Owner);
    }
}
