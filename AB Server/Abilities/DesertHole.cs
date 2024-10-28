using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DesertHoleEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        GateCard target;
        Game game;
        List<Bakugan> ignoredBakugan;


        public Player Owner { get => User.Owner; } bool IsCopy;

        public DesertHoleEffect(Bakugan user, GateCard target, List<Bakugan> ignoredBakugan, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            this.target = target;
            this.ignoredBakugan = ignoredBakugan;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
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

            User.Move(target);

            if (target.Owner != Owner)
                foreach (var bakugan in target.Bakugans.Where(x => x.Owner.SideID != Owner.SideID))
                    if (!ignoredBakugan.Contains(bakugan))
                        bakugan.Boost(new Boost(-50), this);
        }
    }

    internal class DesertHole : AbilityCard, IAbilityCard
    {
        public DesertHole(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            Owner = owner;
            Game = owner.game;
        }

        List<Bakugan> ignoredBakugan;

        public void Setup(bool asCounter)
        {
            ignoredBakugan = new();
            IAbilityCard ability = this;

            Game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject {
                                { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.Bakugans.Any(x=>x.Owner.SideID != Owner.SideID) && (x as GateCard).IsTouching(User.Position as GateCard)).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.Bakugans.Any(x=>x.Owner.SideID != Owner.SideID) && (x as GateCard).IsTouching(User.Position as GateCard)).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        private GateCard target;

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]].Position as GateCard;

            Game.CheckChain(Owner, this, User);
        }

        public void Resolve()
        {
            if (!counterNegated)
                new DesertHoleEffect(User, target, ignoredBakugan, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
            new DesertHoleEffect(User, target, ignoredBakugan, Game, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            ignoredBakugan.Add(bakugan);
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.HasNeighbourEnemies() && !user.Owner.BakuganOwned.Any(x => x.Attribute != Attribute.Subterra);

        public static bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Cast<GateCard>().Any(x => (user.Position as GateCard).IsTouching(x));
    }
}
