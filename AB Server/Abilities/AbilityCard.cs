using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
	internal interface IActive
	{
		public void Negate(bool asCounter = false);
	}
	
    internal class AbilityCard : IAbilityCard, IActive
    {
        static Func<int, Player, IAbilityCard>[] AbilityCtrs =
        [
            //Set 1 Nova abilities
            (cID, owner) => throw new NotImplementedException(), //0
            (cID, owner) => new FireJudge(cID, owner, 1),
            (cID, owner) => new FireTornado(cID, owner, 2),
            (cID, owner) => throw new NotImplementedException(), //3

            //Set 1 Aqua abilities
            (cID, owner) => new WaterRefrain(cID, owner, 4),
            (cID, owner) => throw new NotImplementedException(), //5
            (cID, owner) => throw new NotImplementedException(), //6
            (cID, owner) => new Liquify(cID, owner, 7),

            //Set 1 Darkon abilities
            (cID, owner) => new GrandDown(cID, owner, 8),
            (cID, owner) => throw new NotImplementedException(), //9
            (cID, owner) => new OreganoMurder(cID, owner, 10),
            (cID, owner) => throw new NotImplementedException(), //11

            //Set 1 Zephyros abilities
            (cID, owner) => throw new NotImplementedException(), //12, Air battle, do later
            (cID, owner) => new BlowBack(cID, owner, 13),
            (cID, owner) => new JumpOver(cID, owner, 14),
            (cID, owner) => throw new NotImplementedException(), //15

            //Set 1 Lumina abilities
            (cID, owner) => throw new NotImplementedException(), //16
            (cID, owner) => new LightShield(cID, owner, 17),
            (cID, owner) => new HolyLight(cID, owner, 18),
            (cID, owner) => new ShadeAbility(cID, owner, 19),

            //Set 1 Subterra abilities
            (cID, owner) => throw new NotImplementedException(), //20
            (cID, owner) => new SpiritCanyon(cID, owner, 21),
            (cID, owner) => new DesertHole(cID, owner, 22),
            (cID, owner) => throw new NotImplementedException(), //23

            //Set 1 Garrison abilities
            (cID, owner) => new CoreForcement(cID, owner, 24),
            (cID, owner) => throw new NotImplementedException(),//25

            //Set 1 Griffon abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new TributeSwitch(cID, owner, 27),
            (cID, owner) => new StunningRoar(cID, owner, 28),

            //Set 1 Mantis abilities
            (cID, owner) => throw new NotImplementedException(), //29
            (cID, owner) => throw new NotImplementedException(), //30
            (cID, owner) => new TwinMachete(cID, owner, 31),
            (cID, owner) => throw new NotImplementedException(), //32

            //Set 1 Raptor abilities
            (cID, owner) => throw new NotImplementedException(), //33
            (cID, owner) => throw new NotImplementedException(), //34

            //Set 1 Saurus abilities
            (cID, owner) => throw new NotImplementedException(), //35
            (cID, owner) => throw new NotImplementedException(), //36

            //Set 1 Centipede abilities
            (cID, owner) => throw new NotImplementedException(), //37
            (cID, owner) => throw new NotImplementedException(), //38
            
            //Set 1 Serpent abilities
            (cID, owner) => throw new NotImplementedException(), //39
            (cID, owner) => throw new NotImplementedException(), //40
            (cID, owner) => throw new NotImplementedException(), //41

            //Set 1 Fairy abilities
            (cID, owner) => throw new NotImplementedException(), //42
            (cID, owner) => throw new NotImplementedException(), //43
            (cID, owner) => throw new NotImplementedException(), //44

            //Set 1 Elephant abilities
            (cID, owner) => throw new NotImplementedException(), //45
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }
        public bool counterNegated { get; set; } = false;

        public int TypeId { get; set; }

        public Game Game { get; set; }
        public Player Owner { get; set; }

        public int CardId { get; protected set; }

        public Bakugan User { get; set; }

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPSIE!");
        }

        public void Dispose()
        {
            Game.ActiveZone.Remove(this);
            Owner.AbilityGrave.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
            }
        }

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int TypeId { get; private protected set; }

        public Game Game { get; protected set; }
        public int CardId { get; }
        public Player Owner { get; protected set; }

        protected bool counterNegated { get; set; }

        public bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool IsActivateableFusion(Bakugan user) =>
            throw new NotImplementedException();
        public bool IsActivateableCounter() => IsActivateable();

        public bool BakuganIsValid(Bakugan user) =>
            IsActivateableFusion(user) && user.Owner == Owner && !user.UsedAbilityThisTurn;

        public Bakugan User { get; protected set; }

        public void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;


            Game.CheckChain(Owner, this, user);
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public void Resolve();

        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }
}
