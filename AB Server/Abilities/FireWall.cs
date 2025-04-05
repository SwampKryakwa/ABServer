using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireWall : AbilityCard
    {
        public FireWall(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validBakugans = Game.BakuganIndex.Where(x => x.OnField() && x.InBattle && x.Owner != Owner);

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_SELECT_TARGET", TypeId, (int)Kind, validBakugans)
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup3;
        }

        private Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            if (User.Attribute == Attribute.Nova)
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.OptionSelectionEvent("INFO_PICKER_FIREWALL", 2)
                ));
                Game.AwaitingAnswers[Owner.Id] = HandleOptionSelection;
            }
            else
            {
                Activate(0); // Default to reducing power by 50G if not Nova
            }
        }

        public void HandleOptionSelection()
        {
            int selectedOption = (int)Game.IncomingSelection[Owner.Id]["array"][0]["option"];
            Activate(selectedOption);
        }

        int selectedOption;
        public void Activate(int selectedOption)
        {
            this.selectedOption = selectedOption;

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = User.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new FireWallEffect(User, target, TypeId, IsCopy, selectedOption).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new FireWallEffect(User, target, TypeId, IsCopy, selectedOption).Activate();

        public override void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.InBattle && user.Position.Bakugans.Any(x => x.Owner != Owner);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Position.Bakugans.Any(x => x.Owner != user.Owner);
    }

    internal class FireWallEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;
        int selectedOption;

        public FireWallEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy, int selectedOption)
        {
            User = user;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            this.IsCopy = IsCopy;
            this.selectedOption = selectedOption;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            if (selectedOption == 0)
            {
                // Reduce the power of the target Bakugan by 50G
                target.Boost(new Boost(-50), this);
            }
            else if (selectedOption == 2 && User.Attribute == Attribute.Nova)
            {
                // Set the power of the target Bakugan to its initial value
                foreach (var boost in new List<Boost>(target.Boosts))
                {
                    boost.Active = false;
                    target.Boosts.Remove(boost);
                }
            }
        }
    }
}

