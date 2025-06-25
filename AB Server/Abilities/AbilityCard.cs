using AB_Server.Abilities.Correlations;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AB_Server.Abilities
{
    public enum CardKind : byte
    {
        NormalAbility,
        FusionAbility,
        CorrelationAbility,
        Gate,
        NormalGate
    }

    internal class AbilityCard(int cID, Player owner, int typeId) : IActive, IChainable
    {
        public static (Func<int, Player, AbilityCard> constructor, Func<Bakugan, bool> validTarget)[] AbilityCtrs =
        [
            //Vol. 1 attribute abilities
            ((cID, owner) => new FireJudge(cID, owner, 0), FireJudge.HasValidTargets),
            ((cID, owner) => new SpiritCanyon(cID, owner, 1), SpiritCanyon.HasValidTargets),
            ((cID, owner) => new HolyLight(cID, owner, 2), HolyLight.HasValidTargets),
            ((cID, owner) => new GrandDown(cID, owner, 3), GrandDown.HasValidTargets),
            ((cID, owner) => new WaterRefrain(cID, owner, 4), WaterRefrain.HasValidTargets),
            ((cID, owner) => new Blowback(cID, owner, 5), Blowback.HasValidTargets),

            //Vol. 1 Bakugan abilities
            ((cID, owner) => new SlingBlazer(cID, owner, 6), SlingBlazer.HasValidTargets),
            ((cID, owner) => new LeapSting(cID, owner, 7), Blowback.HasValidTargets),
            ((cID, owner) => new MercilessTriumph(cID, owner, 8), MercilessTriumph.HasValidTargets),

            //Vol. 2 attribute abilities
            ((cID, owner) => new FireWall(cID, owner, 9), FireWall.HasValidTargets),
            ((cID, owner) => new Tunneling(cID, owner, 10), Tunneling.HasValidTargets),
            ((cID, owner) => new LightningTornado(cID, owner, 11), LightningTornado.HasValidTargets),
            ((cID, owner) => new BloomOfAgony(cID, owner, 12), BloomOfAgony.HasValidTargets),
            ((cID, owner) => new IllusiveCurrent(cID, owner, 13), IllusiveCurrent.HasValidTargets),
            ((cID, owner) => new AirBattle(cID, owner, 14), AirBattle.HasValidTargets),

            //Vol. 2 bakugan abilities
            ((cID, owner) => new DefiantCounterattack(cID, owner, 15), DefiantCounterattack.HasValidTargets),
            ((cID, owner) => new CrystalFang(cID, owner, 16), CrystalFang.HasValidTargets),
            ((cID, owner) => new NoseSlap(cID, owner, 17), NoseSlap.HasValidTargets),
            ((cID, owner) => new SaurusGlow(cID, owner, 18), SaurusGlow.HasValidTargets),
            ((cID, owner) => new Dimension4(cID, owner, 19), Dimension4.HasValidTargets),

            //Vol. 2 EX bakugan abilities
            ((cID, owner) => new Enforcement(cID, owner, 20), Enforcement.HasValidTargets),
            ((cID, owner) => new VicariousVictim(cID, owner, 21), VicariousVictim.HasValidTargets),

            //Vol. 3 attribute abilities
            ((cID, owner) => new FireTornado(cID, owner, 22), FireTornado.HasValidTargets),
            ((cID, owner) => new VanguardAdvance(cID, owner, 23), VanguardAdvance.HasValidTargets),
            ((cID, owner) => new IllusiveCurrent(cID, owner, 24), IllusiveCurrent.HasValidTargets),
            ((cID, owner) => new AirBattle(cID, owner, 25), AirBattle.HasValidTargets),
            ((cID, owner) => new LightningTornado(cID, owner, 26), LightningTornado.HasValidTargets),
            ((cID, owner) => new MirrorFlash(cID, owner, 27), MirrorFlash.HasValidTargets),
            ((cID, owner) => new LightningShield(cID, owner, 28), LightningShield.HasValidTargets),
            ((cID, owner) => new MergeShield(cID, owner, 29), MergeShield.HasValidTargets),
            ((cID, owner) => new DiveMirage(cID, owner, 30), DiveMirage.HasValidTargets),
            ((cID, owner) => new BloomOfAgony(cID, owner, 31), BloomOfAgony.HasValidTargets),
            ((cID, owner) => new BlowAway(cID, owner, 32), BlowAway.HasValidTargets),
            ((cID, owner) => new JumpOver(cID, owner, 33), JumpOver.HasValidTargets),
        ];

        public static Func<int, Player, AbilityCard>[] CorrelationCtrs =
        [
            (cID, owner) => new AdjacentCorrelation(cID, owner),
            (cID, owner) => new DiagonalCorrelation(cID, owner),
            (cID, owner) => new TripleNode(cID, owner),
            (cID, owner) => new ElementalFlash(cID, owner)
        ];

        public static AbilityCard CreateCard(Player owner, int cID, int type) =>
            AbilityCtrs[type].constructor.Invoke(cID, owner);
        public bool counterNegated { get; set; } = false;
        public bool IsCopy { get; set; } = false;

        public int TypeId { get; set; } = typeId;
        public virtual CardKind Kind { get; } = CardKind.NormalAbility;

        public Game Game { get; set; } = owner.Game;
        public Player Owner { get; set; } = owner;
        public int EffectId { get; set; }

        public int CardId { get; protected set; } = cID;

        public Bakugan User { get; set; }

        public virtual bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool BakuganIsValid(Bakugan user) =>
            Owner.AbilityBlockers.Count == 0 && IsActivateableByBakugan(user) && user.Owner == Owner;
        public virtual bool IsActivateableByBakugan(Bakugan user) =>
            throw new NotImplementedException();
        public virtual bool IsActivateableCounter() => IsActivateable();

        public static bool HasValidTargets(Bakugan user) => true;

        protected bool asCounter;

        protected int currentTarget;

        protected Selector[] CondTargetSelectors = [];
        protected Selector[] ResTargetSelectors = [];

        public virtual void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            if (!asCounter && Game.CurrentWindow == ActivationWindow.Normal)
                Game.OnCancel[Owner.Id] = Game.ThrowMoveStart;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        protected void RecieveUser()
        {
            currentTarget = 0;
            User = Game.BakuganIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
            SendCondTargetForSelection();
        }

        protected void SendCondTargetForSelection()
        {
            if (!asCounter && Game.CurrentWindow == ActivationWindow.Normal)
                Game.OnCancel[Owner.Id] = Game.ThrowMoveStart;
            if (CondTargetSelectors.Length <= currentTarget) Activate();
            else if (CondTargetSelectors[currentTarget].Condition())
            {
                var currentSelector = CondTargetSelectors[currentTarget];
                if (currentSelector is BakuganSelector bakuganSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                }
                else if (currentSelector is OptionSelector optionSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                }
                else if (currentSelector is YesNoSelector yesNoSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                        ));
                }
                else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).Id] = AcceptCondTarget;
            }
            else
            {
                currentTarget++;
                SendCondTargetForSelection();
            }
        }

        void AcceptCondTarget()
        {
            var currentSelector = CondTargetSelectors[currentTarget];
            if (currentSelector is BakuganSelector bakuganSelector)
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
            else if (currentSelector is GateSelector gateSelector)
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
            else if (currentSelector is AbilitySelector abilitySelector)
            {
                //currently unused
                throw new NotImplementedException();
            }
            else if (currentSelector is ActiveSelector activeSelector)
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
            else if (currentSelector is YesNoSelector yesNoSelector)
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
            else if (currentSelector is OptionSelector optionSelector)
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
            else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
            {
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = bakuganIds.Select(x => Game.BakuganIndex[(int)x]).ToArray();
                Console.WriteLine($"Bakugans selected: {multiBakuganSelector.SelectedBakugans.Length}");
            }
            else
            {
                throw new NotImplementedException();
            }
            currentTarget++;
            SendCondTargetForSelection();
        }

        public virtual void Activate()
        {
            Game.ThrowEvent(new()
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

            EffectId = Game.NextEffectId++;
            Game.ActiveZone.Add(this);
            Game.CardChain.Add(this);
            Game.CheckChain(Owner, this, User);
        }

        public void Resolve()
        {
            currentTarget = 0;
            SendResTargetForSelection();
        }

        protected void SendResTargetForSelection()
        {
            if (currentTarget == ResTargetSelectors.Length)
            {
                Resolution();
                return;
            }
            while (!ResTargetSelectors[currentTarget].HasValidTargets(Game))
            {
                currentTarget++;
                if (currentTarget == ResTargetSelectors.Length) break;
            }
            if (currentTarget == ResTargetSelectors.Length)
            {
                Resolution();
                return;
            }
            if (ResTargetSelectors[currentTarget].Condition())
            {
                var currentSelector = ResTargetSelectors[currentTarget];
                if (currentSelector is BakuganSelector bakuganSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                }
                else if (currentSelector is YesNoSelector yesNoSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                        ));
                }
                else if (currentSelector is OptionSelector optionSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                }
                else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
                {
                    Game.NewEvents[Game.Players.First(currentSelector.ForPlayer).Id].Add(EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).Id] = AcceptResTarget;
            }
            else
            {
                currentTarget++;
                SendResTargetForSelection();
            }
        }

        void AcceptResTarget()
        {
            var currentSelector = ResTargetSelectors[currentTarget];
            if (currentSelector is BakuganSelector bakuganSelector)
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
            else if (currentSelector is GateSelector gateSelector)
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
            else if (currentSelector is AbilitySelector abilitySelector)
            {
                //currently unused
                throw new NotImplementedException();
            }
            else if (currentSelector is ActiveSelector activeSelector)
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
            else if (currentSelector is YesNoSelector yesNoSelector)
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
            else if (currentSelector is OptionSelector optionSelector)
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
            else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
            {
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = bakuganIds.Select(x => Game.BakuganIndex[(int)x]).ToArray();
                Console.WriteLine($"Bakugans selected: {multiBakuganSelector.SelectedBakugans.Length}");
            }
            else
            {
                throw new NotImplementedException();
            }
            currentTarget++;
            SendResTargetForSelection();
        }

        protected void Resolution()
        {
            TriggerEffect();
            Dispose();
            Game.ChainStep();
        }

        public virtual void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            foreach (BakuganSelector selector in CondTargetSelectors.Where(x => x is BakuganSelector))
                if (selector.SelectedBakugan == bakugan)
                    selector.SelectedBakugan = bakugan;
        }

        public virtual void Dispose()
        {
            Discard();
            if (Game.ActiveZone.Contains(this))
                Game.ActiveZone.Remove(this);

            Game.ThrowEvent(new()
            {
                { "Type", "AbilityRemovedActiveZone" },
                { "Id", EffectId },
                { "Card", TypeId },
                { "Owner", Owner.Id }
            });
            Game.ThrowEvent(EventBuilder.SendAbilityToGrave(this));
        }
        public virtual void Discard()
        {
            if (Owner.AbilityHand.Contains(this))
                Owner.AbilityHand.Remove(this);
            if (!IsCopy)
                Owner.AbilityGrave.Add(this);

            Game.ThrowEvent(new()
            {
                ["Type"] = "AbilityRemovedFromHand",
                ["CID"] = CardId,
                ["Owner"] = Owner.Id
            });
        }

        public virtual void Retract()
        {
            Game.ActiveZone.Remove(this);
            if (!IsCopy)
                Owner.AbilityHand.Add(this);

            Game.ThrowEvent(new()
            {
                { "Type", "AbilityRemovedActiveZone" },
                { "Id", EffectId },
                { "Card", TypeId },
                { "Owner", Owner.Id }
            });
            Game.ThrowEvent(EventBuilder.SendAbilityToGrave(this));
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
