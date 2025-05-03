using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SlingBlazer : AbilityCard
    {
        public SlingBlazer(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.InBattle && x.Owner.TeamId != Owner.TeamId},
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacentHorizontally(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
            new SlingBlazerEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, (TargetSelectors[1] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.InBattle && Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget)) && Game.GateIndex.Any(x => x.IsAdjacentHorizontally(user.Position as GateCard));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget));
    }
    internal class SlingBlazerEffect(Bakugan user, Bakugan target, GateCard moveTarget, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan User = user;
        Bakugan target = target;
        GateCard moveTarget = moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            target.Move(moveTarget, new JObject() { ["MoveEffect"] = "LightningChain", ["EffectSource"] = User.BID });
        }
    }
}
