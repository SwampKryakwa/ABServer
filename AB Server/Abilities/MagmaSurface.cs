using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class MagmaSurfaceEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        IGateCard target;
        IGateCard replacement;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public MagmaSurfaceEffect(Bakugan user, IGateCard target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            if (counterNegated) return;

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

            replacement = new NormalGate(Attribute.Subterra, 50, target.CID, game, target.Owner);
            game.Field[target.Position / 10, target.Position % 10] = replacement as GateCard;
            game.GateIndex[target.CID] = replacement;

            replacement.DisallowedPlayers = target.DisallowedPlayers;
            replacement.Position = target.Position;
            replacement.Bakugans = target.Bakugans;
            replacement.ActiveBattle = target.ActiveBattle;

            foreach (var e in game.NewEvents)
            {
                JObject obj = new()
                {
                    { "Type", "GateTypeChange" },
                    { "Pos", target.Position },
                    { "GateData", new JObject {
                        { "Type", 0 },
                        { "Attribute", 5 },
                        { "Power", 100 }
                    }},
                    { "Owner", target.Owner.ID },
                    { "CID", target.CID }
                };
            }

            replacement.Open();

            game.NegatableAbilities.Add(this);
        }

        public void Restore()
        {
            target.DisallowedPlayers = replacement.DisallowedPlayers;
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
                game.Field[replacement.Position / 10, replacement.Position % 10] = target as GateCard;
            }
            game.GateIndex[replacement.CID] = target;
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
            else
                Restore();
        }
    }

    internal class MagmaSurface : AbilityCard, IAbilityCard
    {
        public MagmaSurface(int cID, Player owner)
        {
            CID = cID;
            this.owner = owner;
            game = owner.game;
        }

        public new void Activate()
        {
            game.NewEvents[owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", 6 },
                        { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Subterra & game.GateIndex.Any(y=>(y as GateCard).IsTouching(x.Position) & y.Bakugans.Any(y=>y.Owner.SideID != x.Owner.SideID)) & !x.usedAbilityThisTurn).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "gate_negate_target" },
                        { "Ability", 6 },
                        { "SelectionGates", new JArray(game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.GetTypeID() },
                            { "Pos", x.Position }
                        })) }
                    } }
                }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new MagmaSurfaceEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][0]["bakugan"]], game.GateIndex[(int)game.IncomingSelection[owner.ID]["array"][1]["gate"]], game, 1);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion()
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return game.BakuganIndex.Any(x => x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Subterra & game.GateIndex.Any(y => (y as GateCard).IsTouching(x.Position) & y.Bakugans.Any(y => y.Owner.SideID != x.Owner.SideID)) & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 6;
        }
    }
}
