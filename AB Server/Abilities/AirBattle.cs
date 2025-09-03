using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class AirBattle : AbilityCard
    {
        public AirBattle(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId && x.OnField() && IBakuganContainer.IsAdjacent(x.Position, User.Position) }
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            target.Owner.GateBlockers.Add(this);
            Game.OnLongRangeBattleOver = () =>
            {
                if (User.Position is GateCard positionGate)
                    User.MoveFromFieldToHand(positionGate.EnterOrder);
            };
            Game.StartLongRangeBattle(User, target);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Zephyros)) && Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Zephyros) && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && IBakuganContainer.IsAdjacent(x.Position, user.Position) && user.IsOpponentOf(x));

        [ModuleInitializer]
        internal static void Init() => Register(14, CardKind.NormalAbility, (cID, owner) => new AirBattle(cID, owner, 14));
    }
}
