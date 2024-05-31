using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class NormalGate : GateCard, IGateCard
    {
        public Attribute Attribute;
        public short Power;

        public NormalGate(Attribute attribute, short power, int cID, Game game, Player owner)
        {
            Attribute = attribute;
            Power = power;
            this.game = game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CardId = cID;
        }

        public new int GetTypeID()
        {
            return 0;
        }

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
            game.BakuganIndex.Where(x => x.affectingEffects.Contains(this)).ToList().ForEach(x => x.Boost((short)-Power, this));
            game.BakuganIndex.ForEach(x => x.affectingEffects.Remove(this));

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Open()
        {
            IsOpen = true;

            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                    { "Type", "GateOpenEvent" },
                    { "PosX", Position.X },
                    { "PosY", Position.Y },
                    { "Owner", Owner.ID },
                    { "Bakugan", new JObject {
                        { "Type", 0 },
                        { "Attribute", (int)Attribute },
                        { "Power", Power },
                        { "CID", CardId } }
                    }
                });
            }

            Console.WriteLine("Number of Bakugan: " + Bakugans.Count);

            foreach (var b in Bakugans)
            {
                if (b.Attribute == Attribute)
                {
                    b.Boost(Power, this);
                    b.affectingEffects.Add(this);
                }
            }

            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public new void Remove()
        {
            game.BakuganIndex.ForEach(x => x.affectingEffects.Remove(this));

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, BakuganContainer pos)
        {
            if (target.Attribute == Attribute && pos == this)
            {
                if (!target.affectingEffects.Contains(this))
                {
                    target.affectingEffects.Add(this);
                    target.Boost(Power, this);
                }
            }
            else if (target.affectingEffects.Contains(this) && pos != this)
            {
                target.affectingEffects.Remove(this);
                target.Boost((short)-Power, this);
            }
        }

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (target.Attribute == Attribute && pos == this)
            {
                if (!target.affectingEffects.Contains(this))
                {
                    target.affectingEffects.Add(this);
                    target.Boost(Power, this);
                }
            }
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                target.Boost((short)-Power, this);
            }
        }
    }
}
