using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TsunamiWaveEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;


        public Player Owner { get => User.Owner; }

        public TsunamiWaveEffect(Bakugan user, Game game, int typeID)
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
    internal class TsunamiWave : AbilityCard, IAbilityCard
    {
        public TsunamiWave(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new TsunamiWaveEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Knight && Game.BakuganIndex.Count(y => y != user && y.Attribute == Attribute.Aqua && y.OnField() && y.Owner.SideID == user.Owner.SideID) >= 2 && user.OnField() && user.Attribute == Attribute.Aqua;
    }
}


