namespace AB_Server.Abilities
{
    class BoostEffect(Bakugan user, Bakugan boostTarget, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan target = boostTarget;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            target.Boost(new Boost(boostAmmount), this);
        }
    }

    class BoostMultipleSameEffect(Bakugan user, Bakugan[] boostTargets, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan[] targets = boostTargets;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach (Bakugan target in targets)
                target.Boost(new Boost(boostAmmount), this);
        }
    }

    class BoostMultipleVariousEffect(Bakugan user, Bakugan[] boostTargets, short[] boostAmmounts, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        Bakugan[] targets = boostTargets;
        short[] boostAmmounts = boostAmmounts;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach ((Bakugan target, short boostAmmount) in targets.Zip(boostAmmounts, Tuple.Create))
                target.Boost(new Boost(boostAmmount), this);
        }
    }

    class BoostAllFieldEffect(Bakugan user, short boostAmmount, int typeId, int kindId)
    {

        public int TypeId = typeId;
        public int KindId = kindId;
        Bakugan user = user;
        short boostAmmount = boostAmmount;
        Game game { get => user.Game; }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, KindId, user));
            foreach (Bakugan target in game.BakuganIndex.Where(x => x.OnField()))
                target.Boost(new Boost(boostAmmount), this);
        }
    }
}
