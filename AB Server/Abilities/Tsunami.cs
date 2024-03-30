using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AB_Server.Abilities
{
    internal class TsunamiEffect
    {
        public int TypeID { get; }
        Bakugan user;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public TsunamiEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            if (counterNegated) return;

            foreach (Bakugan b in game.BakuganIndex.Where(x => x != user && x.OnField()))
            {
                b.Destroy((b.Position as GateCard).EnterOrder);
            }
        }
    }
    internal class Tsunami : AbilityCard, IAbilityCard
    {
        public Tsunami(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => x.Type == BakuganType.Knight && Game.BakuganIndex.Count(y => y != x && y.Attribute == Attribute.Aquos && y.OnField() && y.Owner.SideID == x.Owner.SideID) >= 2 && x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Aquos && !x.UsedAbilityThisTurn;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_user" },
                { "Ability", 20 },
                { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });
        }

        public new void Resolve()
        {
            var effect = new TsunamiEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["bakugan"]], Game, 0);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter() => IsActivateable();

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable(bool asFusion) => IsActivateable();

        public new int GetTypeID() => 20;
    }
}


