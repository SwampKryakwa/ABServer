using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard, IGateCard
    {
        public Aquamerge(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 0;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Open()
        {
            game.ContinueGame();
        }

        public new void Remove()
        {
            IsOpen = false;
            TryUnfreeze(this);

            base.Remove();
        }
    }
}
