using AB_Server.Abilities.Correlations;
using AB_Server.Abilities.Fusions;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AB_Server.Abilities
{
    public enum CardKind : byte
    {
        NormalAbility,
        FusionAbility,
        CorrelationAbility,
        Gate
    }

    internal class AbilityCard(int cID, Player owner, int typeId) : IActive, IChainable
    {
        public static (Func<int, Player, AbilityCard> constructor, Func<Bakugan, bool> validTarget)[] AbilityCtrs =
        [
            //Set 1 attribute abilities
            ((cID, owner) => new FireJudge(cID, owner, 0), FireJudge.HasValidTargets),
            ((cID, owner) => new SpiritCanyon(cID, owner, 1), SpiritCanyon.HasValidTargets),
            ((cID, owner) => new HolyLight(cID, owner, 2), HolyLight.HasValidTargets),
            ((cID, owner) => new GrandDown(cID, owner, 3), GrandDown.HasValidTargets),
            ((cID, owner) => new WaterRefrain(cID, owner, 4), WaterRefrain.HasValidTargets),
            ((cID, owner) => new Blowback(cID, owner, 5), Blowback.HasValidTargets),

            //Set 1 Bakugan abilities
            ((cID, owner) => new SlingBlazer(cID, owner, 6), Marionette.HasValidTargets),
            ((cID, owner) => new LeapSting(cID, owner, 7), Blowback.HasValidTargets),
            ((cID, owner) => new MercilessTriumph(cID, owner, 8), MercilessTriumph.HasValidTargets),

            //Set 2 attribute abilities
            ((cID, owner) => new FireWall(cID, owner, 9), FireWall.HasValidTargets),
            ((cID, owner) => new Tunneling(cID, owner, 10), Tunneling.HasValidTargets),
            ((cID, owner) => new LightningTornado(cID, owner, 11), LightningTornado.HasValidTargets),
            ((cID, owner) => new BloomOfAgony(cID, owner, 12), BloomOfAgony.HasValidTargets),
            ((cID, owner) => new IllusiveCurrent(cID, owner, 13), IllusiveCurrent.HasValidTargets),
            ((cID, owner) => new AirBattle(cID, owner, 14), AirBattle.HasValidTargets),

            //Set 2 bakugan abilities
            ((cID, owner) => new DefiantCounterattack(cID, owner, 15), DefiantCounterattack.HasValidTargets),
            ((cID, owner) => new CrystalFang(cID, owner, 16), CrystalFang.HasValidTargets),
            ((cID, owner) => new NoseSlap(cID, owner, 17), NoseSlap.HasValidTargets),
            ((cID, owner) => new SaurusGlow(cID, owner, 18), SaurusGlow.HasValidTargets),
            ((cID, owner) => new Dimension4(cID, owner, 19), Dimension4.HasValidTargets),
        ];

        public static Func<int, Player, AbilityCard>[] CorrelationCtrs =
        [
            (cID, owner) => new AdjacentCorrelation(cID, owner),
            (cID, owner) => new DiagonalCorrelation(cID, owner),
            (cID, owner) => new TripleNode(cID, owner),
            (cID, owner) => new ElementResonance(cID, owner)
        ];

        public static AbilityCard CreateCard(Player owner, int cID, int type) =>
            AbilityCtrs[type].constructor.Invoke(cID, owner);
        public bool counterNegated { get; set; } = false;
        public bool IsCopy { get; set; } = false;

        public int TypeId { get; set; } = typeId;
        public virtual CardKind Kind { get; } = CardKind.NormalAbility;

        public Game Game { get; set; } = owner.game;
        public Player Owner { get; set; } = owner;
        public int EffectId { get; set; }

        public int CardId { get; protected set; } = cID;

        public Bakugan User { get; set; }

        public virtual bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool BakuganIsValid(Bakugan user) =>
            IsActivateableByBakugan(user) && user.Owner == Owner;
        public virtual bool IsActivateableByBakugan(Bakugan user) =>
            throw new NotImplementedException();
        public virtual bool IsActivateableCounter() => IsActivateable();

        public static bool HasValidTargets(Bakugan user) => true;

        protected bool asCounter;

        protected int currentTarget;
        protected Selector[] TargetSelectors = [];

        public virtual void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        protected void RecieveUser()
        {
            currentTarget = 0;
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            SendTargetForSelection();
        }

        protected void SendTargetForSelection()
        {
            if (TargetSelectors.Length == currentTarget) Activate();
            else if (TargetSelectors[currentTarget].Condition())
            {
                var currentSelector = TargetSelectors[currentTarget];
                if (currentSelector is BakuganSelector bakuganSelector)
                {
                    Game.NewEvents[currentSelector.ForPlayer].Add(EventBuilder.SelectionBundler(
                        currentSelector.ClientType switch
                        {
                            "B" => EventBuilder.AnyBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BH" => EventBuilder.HandBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BF" => EventBuilder.FieldBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BG" => EventBuilder.GraveBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator))
                        }
                        ));
                }
                else if (currentSelector is GateSelector gateSelector)
                {
                    Game.NewEvents[currentSelector.ForPlayer].Add(EventBuilder.SelectionBundler(
                        currentSelector.ClientType switch
                        {
                            "G" => throw new NotImplementedException(),
                            "GF" => EventBuilder.FieldGateSelection(currentSelector.Message, TypeId, (int)Kind, Game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GH" => throw new NotImplementedException(),
                            "GG" => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is AbilitySelector abilitySelector)
                {
                    //currently unused
                    throw new NotImplementedException();
                }
                else if (currentSelector is ActiveSelector activeSelector)
                {
                    Game.NewEvents[currentSelector.ForPlayer].Add(EventBuilder.SelectionBundler(
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                }
                else if (currentSelector is OptionSelector optionSelector)
                {
                    Game.NewEvents[currentSelector.ForPlayer].Add(EventBuilder.SelectionBundler(
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                }
                else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
                {
                    Game.NewEvents[currentSelector.ForPlayer].Add(EventBuilder.SelectionBundler(
                        currentSelector.ClientType switch
                        {
                            "MB" => EventBuilder.AnyMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBH" => EventBuilder.HandMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBF" => EventBuilder.FieldMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBG" => EventBuilder.GraveMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator))
                        }
                        ));
                }
                else
                {
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
                }
                Game.OnAnswer[currentSelector.ForPlayer] = AcceptTarget;
            }
            else
            {
                currentTarget++;
                SendTargetForSelection();
            }
        }

        void AcceptTarget()
        {
            var currentSelector = TargetSelectors[currentTarget];
            if (currentSelector is BakuganSelector bakuganSelector)
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[currentSelector.ForPlayer]["array"][0]["bakugan"]];
            else if (currentSelector is GateSelector gateSelector)
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.IncomingSelection[currentSelector.ForPlayer]["array"][0]["gate"]];
            else if (currentSelector is AbilitySelector abilitySelector)
            {
                //currently unused
                throw new NotImplementedException();
            }
            else if (currentSelector is ActiveSelector activeSelector)
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.IncomingSelection[currentSelector.ForPlayer]["array"][0]["active"]);
            else if (currentSelector is OptionSelector optionSelector)
                optionSelector.SelectedOption = (int)Game.IncomingSelection[Owner.Id]["array"][0]["option"];
            else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
            {
                JArray bakuganIds = Game.IncomingSelection[currentSelector.ForPlayer]["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = bakuganIds.Select(x => Game.BakuganIndex[(int)x]).ToArray();
                Console.WriteLine($"Bakugans selected: {multiBakuganSelector.SelectedBakugans.Length}");
            }
            else
            {
                throw new NotImplementedException();
            }
            currentTarget++;
            SendTargetForSelection();
        }

        public virtual void Activate()
        {
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

        public virtual void Resolve()
        {
            TriggerEffect();

            Dispose();
        }

        public virtual void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            foreach (BakuganSelector selector in TargetSelectors.Where(x => x is BakuganSelector))
                if (selector.SelectedBakugan == bakugan)
                    selector.SelectedBakugan = bakugan;
        }

        public virtual void Dispose()
        {
            if (Owner.AbilityHand.Contains(this))
                Owner.AbilityHand.Remove(this);
            if (Game.ActiveZone.Contains(this))
                Game.ActiveZone.Remove(this);
            if (!IsCopy)
                Owner.AbilityGrave.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" },
                    { "Id", EffectId },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
                Game.NewEvents[i].Add(EventBuilder.SendAbilityToGrave(this));
            }
        }
        public virtual void Discard()
        {
            if (Owner.AbilityHand.Contains(this))
                Owner.AbilityHand.Remove(this);
            if (!IsCopy)
                Owner.AbilityGrave.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityRemovedFromHand",
                    ["CID"] = CardId,
                    ["Owner"] = Owner.Id
                });
            }
        }

        public virtual void Retract()
        {
            Game.ActiveZone.Remove(this);
            if (!IsCopy)
                Owner.AbilityHand.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" },
                    { "Id", EffectId },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
                Game.NewEvents[i].Add(EventBuilder.SendAbilityToGrave(this));
            }
        }

        public virtual void TriggerEffect() =>
            throw new NotImplementedException();

        public virtual void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }
}
