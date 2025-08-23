using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class MercilessTriumph : AbilityCard
    {
        public MercilessTriumph(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => IsTargetValid(target, User)}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            target?.Boost(new Boost((short)-target.Power), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Type == BakuganType.Glorius && user.Position is GateCard posGate && posGate.BattleEnding && user.JustEndedBattle && !user.BattleEndedInDraw && Game.BakuganIndex.Any(target => IsTargetValid(target, user));

        static bool IsTargetValid(Bakugan target, Bakugan user) =>
            target.OnField() && target != user;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(target => IsTargetValid(target, user));

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(8, CardKind.NormalAbility, (cID, owner) => new MercilessTriumph(cID, owner, 8));
    }
}




