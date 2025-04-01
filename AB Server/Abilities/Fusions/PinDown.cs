using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class PinDown : FusionAbility
    {
        public PinDown(int cID, Player owner)
        {
            TypeId = 8; // Уникальный идентификатор для Pin Down
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(LeapSting);
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = PickTarget;
        }

        public void PickTarget()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_TARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.OnField()))
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        Bakugan target;
        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            FusedTo.Discard();

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
                new PinDownEffect(User, target, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new PinDownEffect(User, target, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Laserman && user.OnField() && Game.BakuganIndex.Count(x => x.OnField()) >= 2;
    }

    internal class PinDownEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public PinDownEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            this.user = user;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                    {
                        { "Type", "FusionAbilityActivateEffect" },
                        { "Kind", 1 },
                        { "Card", TypeId },
                        { "UserID", user.BID },
                        { "User", new JObject {
                            { "Type", (int)user.Type },
                            { "Attribute", (int)user.Attribute },
                            { "Treatment", (int)user.Treatment },
                            { "Power", user.Power }
                        }},
                        { "TargetID", target.BID },
                        { "Target", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power }
                        }}
                    });
            }

            if (target.Position is GateCard targetGate)
            {
                foreach (var bakugan in targetGate.Bakugans.Where(b => b != target))
                {
                    int powerDifference = 400 - bakugan.Power;
                    bakugan.Boost(new Boost((short)powerDifference), this);
                }
            }
        }
    }
}
