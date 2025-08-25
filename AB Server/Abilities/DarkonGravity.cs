using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class DarkonGravity : AbilityCard
    {
        public DarkonGravity(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.Position != User.Position && x.OnField() }
            ];
        }

        public override void TriggerEffect() =>
            (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Move(User.Position as GateCard, new JObject { ["MoveEffect"] = "LightningChain", ["Attribute"] = (int)User.BaseAttribute, ["EffectSource"] = User.BID });

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Darkon) && Owner.Bakugans.Count == 0 && Game.BakuganIndex.Any(x => x.OnField() && x.Position != user.Position);

        [ModuleInitializer]
        internal static void Init() => Register(28, CardKind.NormalAbility, (cID, owner) => new DarkonGravity(cID, owner, 28));
    }
}
