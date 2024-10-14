using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {
        static Func<int, Player, IAbilityCard>[] AbilityCtrs =
        [
            //Set 1 Nova abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new FireJudge(cID, owner, 1),
            (cID, owner) => new FireTornado(cID, owner, 2),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Aqua abilities
            (cID, owner) => new WaterRefrain(cID, owner, 4),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new Liquify(cID, owner, 7),

            //Set 1 Darkon abilities
            (cID, owner) => new GrandDown(cID, owner, 8),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new OreganoMurder(cID, owner, 10),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Zephyros abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new BlowBack(cID, owner, 13),
            (cID, owner) => new JumpOver(cID, owner, 14),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Lumina abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new LightShield(cID, owner, 17),
            (cID, owner) => new HolyLight(cID, owner, 18),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Subterra abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new SpiritCanyon(cID, owner, 21),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Garrison abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Griffon abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Mantis abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => new TwinMachete(cID, owner, 31),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Raptor abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Saurus abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Centipede abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            
            //Set 1 Serpent abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Fairy abilities
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),
            (cID, owner) => throw new NotImplementedException(),

            //Set 1 Elephant abilities
            (cID, owner) => throw new NotImplementedException(),
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }

        public int TypeId =>
            throw new NotImplementedException();

        public Game Game { get; set; }
        public Player Owner { get; set; }

        public int CardId { get; protected set; }

        protected bool counterNegated = false;

        public Bakugan User { get; set; }

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPSIE!");
        }

        public void Negate()
        {
            counterNegated = true;
        }

        protected void Dispose()
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
        public int TypeId =>
            throw new NotImplementedException();

        public Game Game { get; protected set; }
        public int CardId { get; }
        public Player Owner { get; protected set; }

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

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPS!");
        }
    }
}
