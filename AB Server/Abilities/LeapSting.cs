using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LeapSting : AbilityCard
    {
        public LeapSting(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.SideID != Owner.SideID && x.OnField() && x.Position != User.Position}
            ];
        }

        public override void TriggerEffect() =>
            new LeapStingEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField() && Game.BakuganIndex.Any(x => x.Owner.SideID != Owner.SideID && x.OnField() && x.Position != user.Position);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && x.Position != user.Position && user.IsEnemyOf(x));
    }

    internal class LeapStingEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public LeapStingEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            if (target.Power < User.Power)
                if (target.Position is GateCard positionGate)
                    target.DestroyOnField(positionGate.EnterOrder);
        }
    }
}
