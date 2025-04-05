using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TunnelingEffect
    {
        public int TypeId { get; }
        Bakugan User;
        GateCard moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public TunnelingEffect(Bakugan user, GateCard moveTarget, int typeID, bool IsCopy)
        {
            User = user;
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
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Move(moveTarget);
        }
    }

    internal class Tunneling : AbilityCard
    {
        public Tunneling(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        GateCard moveTarget;

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldGateSelection("INFO_ABILITY_DESTINATIONTARGET", TypeId, (int)Kind, Game.GateIndex.Where(x => x.Position.X == (User.Position as GateCard).Position.X && x != User.Position && !x.IsTouching(User.Position as GateCard)))
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            moveTarget = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = User.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new TunnelingEffect(User, moveTarget, TypeId, IsCopy).Activate();
            Dispose();
        }

        public override void DoubleEffect() =>
                new TunnelingEffect(User, moveTarget, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Attribute == Attribute.Subterra && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.OnField() && user.Game.GateIndex.Any(gate => gate.Position.Y == (user.Position as GateCard).Position.Y && !gate.IsTouching(user.Position as GateCard));
    }
}
