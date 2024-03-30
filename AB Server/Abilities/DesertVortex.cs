using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class DesertVortexEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        IGateCard target;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public DesertVortexEffect(Bakugan user, IGateCard target, Game game, int typeID)
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

            User.Move(target as GateCard);

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

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }

    internal class DesertVortex : AbilityCard, IAbilityCard
    {
        public DesertVortex(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => x.Owner == owner && x.OnField() && x.Attribute == Attribute.Subterra && !x.UsedAbilityThisTurn && x.HasNeighbourEnemies();
        }

        public new void Activate()
        {
            List<Bakugan> users = Game.BakuganIndex.Where(BakuganIsValid).ToList();
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 7 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", 7 },
                        { "SelectionBakugans", new JArray(users.Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G?" },
                        { "Message", "gate_move_target" },
                        { "Ability", 7 },
                        { "SelectionRange", "TGHE" },
                        { "CompareTo", 0 }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new DesertVortexEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]], Game.Field[(int)Game.IncomingSelection[Owner.ID]["array"][1]["pos"] / 10, (int)Game.IncomingSelection[Owner.ID]["array"][1]["pos"] % 10], Game, 1);

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
            List<GateCard> notNull = Game.Field.Cast<GateCard>().Where(x => x != null).ToList();
            List<GateCard> hasEnemies = notNull.Where(x=>x.Bakugans.Any(z => z.Owner.SideID != Owner.SideID)).ToList();
            List<Bakugan> possibleUsers = Game.BakuganIndex.Where(x => x.Owner == Owner && x.Attribute == Attribute.Subterra && x.OnField()).ToList(); 
            foreach (Bakugan b in possibleUsers)
            {
                if (hasEnemies.Any(x => x.IsTouching(b.Position as GateCard))) return true;
            }
            return false;
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 7;
        }
    }
}
