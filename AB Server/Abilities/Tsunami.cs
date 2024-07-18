using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TsunamiEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;


        public Player Owner { get => User.Owner; }

        public TsunamiEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            foreach (Bakugan b in game.BakuganIndex.Where(x => x != User && x.OnField()))
            {
                b.Destroy((b.Position as GateCard).EnterOrder);
            }
        }
    }
    internal class Tsunami : AbilityCard, IAbilityCard
    {
        public Tsunami(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new TsunamiEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Knight && Game.BakuganIndex.Count(y => y != user && y.Attribute == Attribute.Aquos && y.OnField() && y.Owner.SideID == user.Owner.SideID) >= 2 && user.OnField() && user.Attribute == Attribute.Aquos;

        public new int TypeId { get; } = 20;
    }
}


