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
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.InBattle && x.Owner.SideID != Owner.SideID},
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
    internal class SlingBlazerEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan target;
        GateCard moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public SlingBlazerEffect(Bakugan user, Bakugan target, GateCard moveTarget, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.moveTarget = moveTarget;

            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            target.Move(moveTarget);
        }
    }
}
