using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions
{
    internal class Tremors : FusionAbility
    {
        public Tremors(int cID, Player owner) : base(cID, owner, 5, typeof(NoseSlap))
        {
            CondTargetSelectors =
            [
                new MultiBakuganSelector() { ClientType = "MBF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGETS", TargetValidator = x => x.OnField() && !(x.Position as GateCard)!.IsAdjacent((User.Position as GateCard)!) && x.Position != User.Position && x.IsOpponentOf(User) }
            ];
        }

        public override void TriggerEffect()
        {
            var targets = (CondTargetSelectors[0] as MultiBakuganSelector)!.SelectedBakugans;
            Game.OnLongRangeBattleOver = () =>
            {
                foreach (var target in targets.Where(x => x.OnField()))
                    target.Boost(new Boost((short)-target.Power), this);
            };
            Game.StartLongRangeBattle(User, targets);
            foreach (var target in targets)
            {
                if (target.Power < User.Power)
                {
                    // Destroy the target Bakugan if it is on the field
                    if (target.Position is GateCard positionGate)
                        target.MoveFromFieldToDrop(positionGate.EnterOrder);
                }
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.Position is GateCard userPos && Game.BakuganIndex.Any(possibleTarget => possibleTarget.Position is GateCard targetPos && !targetPos.IsAdjacent(userPos) && targetPos != userPos && possibleTarget.IsOpponentOf(user));

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(5, (cID, owner) => new Tremors(cID, owner));
    }
}
