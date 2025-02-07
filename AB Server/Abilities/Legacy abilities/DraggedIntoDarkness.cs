using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace AB_Server.Abilities
{
    internal class DraggedIntoDarknessEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        List<Bakugan> ignoredBakugan;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public DraggedIntoDarknessEffect(Bakugan user, List<Bakugan> ignoredBakugan, Game game, int typeID, bool IsCopy)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
            this.game = game;
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

            Bakugan.MultiMove(game, User.Position as GateCard, MoveSource.Effect, game.BakuganIndex.Where(x => x != User && x.OnField() && !ignoredBakugan.Contains(x)).ToArray());
        }
    }

    internal class DraggedIntoDarkness : AbilityCard
    {
        public DraggedIntoDarkness(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        List<Bakugan> ignoredBakugan;

        public void Setup(bool asCounter)
        {
            AbilityCard ability = this;

            ignoredBakugan = new();
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = ability.Activate;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new DraggedIntoDarknessEffect(User, ignoredBakugan, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new DraggedIntoDarknessEffect(User, ignoredBakugan, Game, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            ignoredBakugan.Add(bakugan);
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Centipede && user.Attribute == Attribute.Darkon && user.Owner.Bakugans.Count == 0;


    }
}
