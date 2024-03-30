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
            user.UsedAbilityThisTurn = true;
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
            game.Field[target.Position.X, target.Position.Y] = replacement as GateCard;
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
                    { "PosX", target.Position.X }, { "PosY", target.Position.Y },
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
                game.Field[replacement.Position.X, replacement.Position.Y] = target as GateCard;
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
            Owner = owner;
            Game = owner.game;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", 6 },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Subterra && Game.GateIndex.Any(y=>(y as GateCard).IsTouching(x.Position as GateCard) && y.Bakugans.Any(y=>y.Owner.SideID != x.Owner.SideID)) && !x.UsedAbilityThisTurn).Select(x =>
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
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.GetTypeID() },
                            { "PosX", x.Position.X }, { "PosY", x.Position.Y }
                        })) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new MagmaSurfaceEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]], Game.GateIndex[(int)Game.IncomingSelection[Owner.ID]["array"][1]["gate"]], Game, 1);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Subterra && Game.GateIndex.Any(y => (y as GateCard).IsTouching(x.Position as GateCard) && y.Bakugans.Any(y => y.Owner.SideID != x.Owner.SideID)) && !x.UsedAbilityThisTurn);
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
