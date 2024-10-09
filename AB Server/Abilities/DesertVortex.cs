using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DesertVortexEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        GateCard target;
        IGateCard source;
        Game game;


        public Player Owner { get => User.Owner; }

        public DesertVortexEffect(Bakugan user, GateCard target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            target = target;
            source = user.Position as IGateCard;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            User.Move(target);

            game.BattleOver += Return;
            game.BakuganDestroyed += (Bakugan target, ushort owner) => { if (target == User) Negate(); };
            game.BakuganReturned += (Bakugan target, ushort owner) => { if (target == User) Negate(); };

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 7 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
        }

        public void Return(IGateCard target, ushort winner)
        {
            if (winner == User.Owner.ID && source.OnField)
                User.Move(this.target);
            game.BattleOver -= Return;
        }

        public void Negate()
        {
            game.BattleOver -= Return;
        }
    }

    internal class DesertVortex : AbilityCard, IAbilityCard
    {
        public DesertVortex(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            Game.AbilityChain.Add(this);
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).ToList().Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } }
                }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            Game.AbilityChain.Add(this);
            Game.NewEvents[Owner.ID].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => (Math.Abs((x.Position as GateCard).Position.X - (User.Position as GateCard).Position.X) + Math.Abs((x.Position as GateCard).Position.Y - (User.Position as GateCard).Position.Y)) == 1).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.ID].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => (Math.Abs((x.Position as GateCard).Position.X - (User.Position as GateCard).Position.X) + Math.Abs((x.Position as GateCard).Position.Y - (User.Position as GateCard).Position.Y)) == 1).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        private GateCard target;

        public new void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]].Position as GateCard;

            Game.CheckChain(Owner, this, User);
        }

        public void Resolve(Bakugan user)
        {
            if (!counterNegated)
                new DesertVortexEffect(user, target, Game, 1).Activate();

            Dispose();
        }

        public new bool IsActivateable()
        {
            List<GateCard> notNull = Game.Field.Cast<GateCard>().Where(x => x != null).ToList();
            List<GateCard> hasEnemies = notNull.Where(x => x.Bakugans.Any(z => z.Owner.SideID != Owner.SideID)).ToList();
            List<Bakugan> possibleUsers = Game.BakuganIndex.Where(x => x.Owner == Owner && x.Attribute == Attribute.Subterra && x.OnField()).ToList();
            foreach (Bakugan b in possibleUsers)
            {
                if (hasEnemies.Any(x => x.IsTouching(b.Position as GateCard))) return true;
            }
            return false;
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Subterra && user.HasNeighbourEnemies();

        public new int TypeId { get; private protected set; } = 7;
    }
}
