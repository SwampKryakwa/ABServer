namespace AB_Server.Gates
{
    internal class AttributeHazard : GateCard, IGateCard
    {
        public Attribute Attribute;
        public AttributeHazard(int cID, Player owner, Attribute attribute)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CardId = cID;
            Attribute = attribute;
        }

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this)))
            {
                b.affectingEffects.Remove(this);
                b.Attribute = b.BaseAttribute;
            }

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Set(int posX, int posY)
        {
            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            base.Set(posX, posY);
        }

        public void Trigger()
        {
            if (!IsOpen && !Negated) Open();
        }

        public new void Open()
        {
            IsOpen = true;
            Bakugans[0].affectingEffects.Add(this);
            Bakugans[0].Attribute = Attribute;

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;

            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public new void Remove()
        {
            IsOpen = false;
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this)))
            {
                b.affectingEffects.Remove(this);
                b.Attribute = b.BaseAttribute;
            }

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, BakuganContainer pos)
        {
            if (pos == this) Trigger();
        }

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (pos == this) Trigger();
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                target.Attribute = target.BaseAttribute;
            }
        }
    }
}
