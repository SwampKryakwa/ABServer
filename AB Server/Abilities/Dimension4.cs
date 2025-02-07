using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Dimension4Effect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        Bakugan User;
        Bakugan Target;
        Game game;

        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public Dimension4Effect(Bakugan user, Bakugan target, Game game, int typeID, bool IsCopy)
        {
            User = user;
            Target = target;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            this.IsCopy = IsCopy;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            game.ActiveZone.Add(this);

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
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" },
                    { "IsCopy", IsCopy },
                    { "Card", TypeId },
                    { "Id", EffectId },
                    { "Owner", Owner.Id },
                    { "Kind", 0 }
                });
            }

            foreach (var boost in Target.Boosts.ToList())
            {
                if (boost.Active)
                {
                    boost.Active = false;
                    Target.RemoveBoost(boost, this);
                }
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
            }
        }
    }

    internal class Dimension4 : AbilityCard
    {
        public Dimension4(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
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

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.InBattle && x.Owner.SideID != Owner.SideID).Select(x =>
                            new JObject { { "Type", (int)x.Type },
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

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Resolve()
        {
            if (!counterNegated)
            {
                Setup(false);
            }
            else
            {
                Dispose();
            }
        }

        public new void DoubleEffect() =>
            new Dimension4Effect(User, Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]], Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Lucifer && user.InBattle;

        public bool BakuganIsValid(Bakugan bakugan) =>
            bakugan.Game.BakuganIndex.Any(bakugan => bakugan.InBattle && bakugan.Owner.SideID != Owner.SideID);
    }
}