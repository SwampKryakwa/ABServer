using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class AirBattle : AbilityCard
    {
        public AirBattle(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId && x.OnField() && IsAdjacent(x.Position, User.Position)}
            ];
        }

        public override void TriggerEffect() =>
            new AirBattleEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Zephyros)) && Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Zephyros) && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && IsAdjacent(x.Position, user.Position) && user.IsOpponentOf(x));

        private static bool IsAdjacent(IBakuganContainer pos1, IBakuganContainer pos2)
        {
            if (pos1 is GateCard gate1 && pos2 is GateCard gate2)
                return GateCard.AreAdjacent(gate1, gate2);
            return false;
        }
    }
    internal class AirBattleEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        int typeId { get; } = typeID;
        Bakugan user = user;
        Bakugan target = target;
        Game game { get => user.Game; }
        Player owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            

            target.Owner.GateBlockers.Add(this);
            game.OnLongRangeBattleOver = AfterBattleOver;
            game.StartLongRangeBattle(user, target);
        }

        public void AfterBattleOver()
        {
            if (user.Position is GateCard positionGate)
                user.MoveFromFieldToHand(positionGate.EnterOrder);
        }
    }
}
