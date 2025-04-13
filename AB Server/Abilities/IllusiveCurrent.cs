using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class IllusiveCurrent : AbilityCard
    {
        public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = owner.Id, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => User.IsAttribute(Attribute.Aqua)
                ?  x.InHand()
                :  x.InHand() && x.MainAttribute == Attribute.Aqua}
            ];
        }

        public override void TriggerEffect() =>
            new IllusiveCurrentEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField();
    }

    internal class IllusiveCurrentEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public IllusiveCurrentEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
        {
            User = user;
            this.selectedBakugan = selectedBakugan;
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

            if (User.Position is GateCard positionGate)
            {
                User.ToHand(positionGate.EnterOrder);
                selectedBakugan.AddFromHand(positionGate);
            }
        }
    }
}

