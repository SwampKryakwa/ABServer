using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class MagmaProminenceEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        IGateCard target;
        IGateCard replacement;
        Game game;


        public Player Owner { get => User.Owner; } bool IsCopy;

        public MagmaProminenceEffect(Bakugan user, IGateCard target, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            target.Negate();

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 6 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            replacement = new NormalGate(Attribute.Subterra, 50, target.CardId, game, target.Owner);
            game.Field[target.Position.X, target.Position.Y] = replacement as GateCard;
            game.GateIndex[target.CardId] = replacement;

            replacement.Position = target.Position;
            replacement.Bakugans = target.Bakugans;
            replacement.ActiveBattle = target.ActiveBattle;

            foreach (var e in game.NewEvents)
            {
                JObject obj = new()
                {
                    { "Type", "GateTypeChange" },
                    { "PosX", target.Position.X }, { "PosY", target.Position.Y },
                    { "GateData", new JObject {
                        { "Type", 0 },
                        { "Attribute", 5 },
                        { "Power", 100 }
                    }},
                    { "Owner", target.Owner.Id },
                    { "CID", target.CardId }
                };
            }

            replacement.Open();
        }

        public void Restore()
        {
            target.Position = replacement.Position;
            target.Bakugans = replacement.Bakugans;
            target.ActiveBattle = replacement.ActiveBattle;

            if (replacement.Owner.GateGrave.Contains(replacement))
            {
                replacement.Owner.GateGrave.Remove(replacement);
                replacement.Owner.GateGrave.Add(target);
            }
            if (replacement.Owner.GateHand.Contains(replacement))
            {
                replacement.Owner.GateHand.Remove(replacement);
                replacement.Owner.GateHand.Add(target);
            }
            if (game.Field.Cast<GateCard>().Contains(replacement))
            {
                game.Field[replacement.Position.X, replacement.Position.Y] = target as GateCard;
            }
            game.GateIndex[replacement.CardId] = target;
        }
    }

    internal class MagmaProminence : AbilityCard
    {
        public MagmaProminence(int cID, Player owner)
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
                { "Count", 2 },
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
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "INFO_GATENEGATETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X }, { "PosY", x.Position.Y }
                        })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(AbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "INFO_GATENEGATETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X }, { "PosY", x.Position.Y }
                        })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = ActivateFusion;
        }

        private IGateCard target;

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            target = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][1]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public void ActivateFusion()
        {
            target = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new MagmaProminenceEffect(User, target, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Subterra && Game.GateIndex.Any(y => y.IsOpen);
    }
}
