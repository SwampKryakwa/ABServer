using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class NoseSlap : AbilityCard
    {
        public NoseSlap(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.OnField() && x.Owner != Owner && (x.Position as GateCard).IsAdjacentVertically(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
            new NoseSlapEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField() && Game.GateIndex.Any(x => x.OnField && x.IsTouching(user.Position as GateCard) && x.Bakugans.Any(user.IsEnemyOf));

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.GateIndex.Any(x => x.OnField && x.IsAdjacentVertically(user.Position as GateCard) && x.Bakugans.Any(user.IsEnemyOf));
    }

    internal class NoseSlapEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public NoseSlapEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;

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

            // Compare the powers of the user and the target Bakugan
            if (target.Power < User.Power)
            {
                // Destroy the target Bakugan if it is on the field
                if (target.Position is GateCard positionGate)
                    target.DestroyOnField(positionGate.EnterOrder);
            }
        }
    }
}
