using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class LeapSting : AbilityCard
    {
        public LeapSting(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId && x.Position is GateCard enemGate && User.Position is GateCard userGate && enemGate.IsAdjacent(userGate)}
            ];
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            target.AbilityBlockers.Add(this);
            Game.OnLongRangeBattleOver = () =>
            {
                target.AbilityBlockers.Remove(this);
                if (User.Position is GateCard positionGate)
                    User.MoveFromFieldToHand(positionGate.EnterOrder);
            };
            Game.StartLongRangeBattle(User, target);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField() && Game.BakuganIndex.Any(x => x.Owner.TeamId != Owner.TeamId && x.OnField() && x.Position != user.Position);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && x.Position != user.Position && user.IsOpponentOf(x));

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(7, CardKind.NormalAbility, (cID, owner) => new LeapSting(cID, owner, 7));
    }

    internal class LeapStingEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        int TypeId { get; } = typeID;
        Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            target.AbilityBlockers.Add(this);
            game.OnLongRangeBattleOver = () =>
            {
                target.AbilityBlockers.Remove(this);
                if (User.Position is GateCard positionGate)
                    User.MoveFromFieldToHand(positionGate.EnterOrder);
            };
            game.StartLongRangeBattle(User, target);
        }
    }
}
