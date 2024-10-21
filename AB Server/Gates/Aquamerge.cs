using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard, IGateCard
    {
        Dictionary<Bakugan, Attribute> affectedBakugan;

        public Aquamerge(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
                DisallowedPlayers[i] = false;

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
            affectedBakugan = new();

            foreach (var bakugan in Bakugans.Where(x => x.Attribute != Attribute.Subterra))
                affectedBakugan.Add(bakugan, bakugan.ChangeAttribute(Attribute.Aqua, this));

            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;

            base.Open();

            game.ContinueGame();
        }

        public new void Remove()
        {
            foreach (var bakugan in affectedBakugan.Keys)
                bakugan.ChangeAttribute(affectedBakugan[bakugan], this);

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, BakuganContainer pos)
        {
            if (pos == this && target.Attribute != Attribute.Subterra)
                affectedBakugan.Add(target, target.ChangeAttribute(Attribute.Aqua, this));

            else if (affectedBakugan.Keys.Contains(target) && pos != this)
            {
                target.ChangeAttribute(affectedBakugan[target], this);
                affectedBakugan.Remove(target);
            }
        }

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (pos == this && target.Attribute != Attribute.Subterra)
                affectedBakugan.Add(target, target.ChangeAttribute(Attribute.Aqua, this));
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (affectedBakugan.Keys.Contains(target))
            {
                target.ChangeAttribute(affectedBakugan[target], this);
                affectedBakugan.Remove(target);
            }
        }
    }
}
