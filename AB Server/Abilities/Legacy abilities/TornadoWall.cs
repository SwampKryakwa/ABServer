using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TornadoWallEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public TornadoWallEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            target = target;
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
                    { "Card", 18 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.OnField()))
            {
                if (b.Owner == User.Owner && b.Attribute == Attribute.Zephyros)
                {
                    b.Boost(new Boost(80), this);
                    b.affectingEffects.Add(this);
                    continue;
                }
                if (b.Owner.SideID != User.Owner.SideID)
                {
                    b.Boost(new Boost(-80), this);
                    b.affectingEffects.Add(this);
                }
            }

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (leaver.affectingEffects.Contains(this))
            {
                leaver.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class TornadoWall : AbilityCard
    {
        public TornadoWall(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }
        public void Setup(bool asCounter)
        {
            AbilityCard ability = this;
            
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionFieldBakugans", new JArray(Game.BakuganIndex.Where(x=> ability.BakuganIsValid(x) && x.OnField()).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) },
                        { "SelectionHandBakugans", new JArray(Game.BakuganIndex.Where(x=> ability.BakuganIsValid(x) && x.InHand()).Select(x =>
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
                new TornadoWallEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            !user.InGrave() && user.Attribute == Attribute.Zephyros;
    }
}
