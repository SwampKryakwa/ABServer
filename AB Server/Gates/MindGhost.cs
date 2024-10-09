using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class MindGhost : GateCard, IGateCard
    {
        public MindGhost(int cID, Player owner)
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

        public new int TypeId { get; private protected set; } = 3;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Open()
        {
            IsOpen = true;
            if (Bakugans.Count < 3)
                Freeze(this);
        }

        public new void Remove()
        {
            IsOpen = false;
            TryUnfreeze(this);

            base.Remove();
        }

        public new void DetermineWinner()
        {
            if (!IsOpen)
            {
                base.DetermineWinner();
                return;
            }

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.Destroy(EnterOrder);
            }

            foreach (List<JObject> e in game.NewEvents)
            {
                e.Add(new JObject
                {
                    { "Type", "BattleOverAllLoser" },
                });
            }
        }
    }
}
