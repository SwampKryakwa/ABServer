using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FrameFireEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        IAbilityCard target;
        bool isCounter;
        Game game;

        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public FrameFireEffect(Bakugan user, IAbilityCard target, bool isCounter, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            this.target = target;
            this.isCounter = isCounter;
            TypeId = typeID;

            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

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

            User.Boost(new Boost(100), this);
            target.DoNotAffect(User);
        }
    }

    internal class FrameFire : AbilityCard, IAbilityCard
    {
        public FrameFire(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        private IAbilityCard target;
        private bool isCounter = false;

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            isCounter = asCounter;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
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

            Game.awaitingAnswers[Owner.Id] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.Where(x => x.ActiveType == ActiveType.Card).ToArray())
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.Where(x => x.ActiveType == ActiveType.Card).ToArray())
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Activate()
        {
            target = (IAbilityCard)Game.ActiveZone.First(x => x.EffectId == (int)Game.IncomingSelection[Owner.Id]["array"][0]["active"]);

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new FrameFireEffect(User, target, isCounter, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new FrameFireEffect(User, target, isCounter, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) => user.InBattle && user.Attribute == Attribute.Nova && user.Type == BakuganType.Raptor;

        public static bool HasValidTargets(Bakugan user) =>
            user.Game.ActiveZone.Any(x => x.ActiveType == ActiveType.Card);
    }
}
