using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions
{
    internal class HarmonicGrace : FusionAbility
    {
        public HarmonicGrace(int cID, Player owner) : base(cID, owner, 14, typeof(ScarletWaltz))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner == Owner && x != User}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.Position is GateCard posGate && target.OnField())
            {
                target.Move(posGate, new JObject { ["MoveEffect"] = "Slide" });
                target.Boost(new Boost((short)User.Power), this);
                User.MoveFromFieldToHand(posGate.EnterOrder);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Fairy && !user.InBattle && Game.BakuganIndex.Any(x => x.OnField() && x != user && x.Owner == Owner);

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(13, (cID, owner) => new HarmonicGrace(cID, owner));
    }
}
