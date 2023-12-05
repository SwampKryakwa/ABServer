namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {
        static Func<int, Player, IAbilityCard>[] AbilityCtrs = new Func<int, Player, IAbilityCard>[]
        {
            (x, y) => new FireJudge(x, y),
            (x, y) => new FireTornado(x, y),
            (x, y) => new Backfire(x, y),
            (x, y) => new RapidFire(x, y)
        };

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }
        private protected Game game;
        private protected Player owner;

        public int CID { get; protected set; }
        public int GetTypeID()
        {
            throw new NotImplementedException();
        }
        public void Activate()
        {
            throw new NotImplementedException();
        }
        public void ActivateCounter()
        {
            throw new NotImplementedException();
        }
        public void ActivateFusion()
        {
            throw new NotImplementedException();
        }
        protected void Dispose()
        {
            owner.AbilityHand.Remove(this);
            owner.AbilityGrave.Add(this);
        }

        public void Resolve()
        {
            throw new NotImplementedException();
        }

        private protected int iter = 0;

        public bool Iter()
        {
            iter++;
            if (iter == game.PlayerCount)
            {
                iter = 0;
                return true;
            }
            return false;
        }

        public bool IsActivateable()
        {
            throw new NotImplementedException();
        }
        public bool IsActivateable(bool asFusion)
        {
            throw new NotImplementedException();
        }

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int CID { get; }
        public int GetTypeID();
        public void Activate();
        public void ActivateCounter();
        public void ActivateFusion();

        public bool Iter();
        public void Resolve();

        public bool IsActivateable();
        public bool IsActivateable(bool asFusion);
    }

    interface INegatable
    {
        public int TypeID { get; }
        public Player GetOwner();
        public void Negate(bool asCounter);
    }
}
