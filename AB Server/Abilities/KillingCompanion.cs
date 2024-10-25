using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DoomCompanionEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        public Bakugan User;
        Bakugan target;
        Game game;

        public Player Owner { get => User.Owner; }

        public DoomCompanionEffect(Bakugan user, Bakugan target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
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
                    { "Card", TypeId },
                    { "Id", EffectId },
                    { "Owner", Owner.Id }
                });
            }

            game.BakuganMoved += MoveFromBattleTurnover;
            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += OnBakuganDestroyed;
        }

        private void MoveFromBattleTurnover(Bakugan target, BakuganContainer pos)
        {
            if (target == User || target == target)
            {
                game.ActiveZone.Remove(this);

                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed += OnBakuganDestroyed;

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

        //remove when goes to hand
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User || leaver == target)
            {
                game.ActiveZone.Remove(this);

                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed += OnBakuganDestroyed;

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

        private void OnBakuganDestroyed(Bakugan target, ushort owner)
        {
            if (target == User)
            {
                this.target.Destroy((this.target.Position as GateCard).EnterOrder);

                game.ActiveZone.Remove(this);

                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed += OnBakuganDestroyed;

                for (int i = 0; i < game.NewEvents.Length; i++)
                {
                    game.NewEvents[i].Add(new()
                    {
                        { "Type", "EffectRemovedActiveZone" },
                        { "Id", EffectId }
                    });
                }
            }
            else if (target == this.target)
            {
                game.ActiveZone.Remove(this);

                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed += OnBakuganDestroyed;

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

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganReturned -= FieldLeaveTurnover;
            game.BakuganDestroyed += OnBakuganDestroyed;

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

    internal class KillingCompanion : AbilityCard, IAbilityCard
    {

        public KillingCompanion(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        private Bakugan target;

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            
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

            Game.awaitingAnswers[Owner.Id] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(User.Position.Bakugans.Where(x=>x.Owner != Owner).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
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
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(User.Position.Bakugans.Where(x=>x.Owner != Owner).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new DoomCompanionEffect(User, target, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new DoomCompanionEffect(User, target, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) => user.InBattle && !user.Owner.BakuganOwned.Any(x => x.Attribute != Attribute.Nova);
    }
}
