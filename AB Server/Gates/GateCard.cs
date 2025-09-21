using AB_Server.Abilities;
using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    abstract class GateCard : IBakuganContainer, IActive, IChainable
    {
        public Bakugan User { get; set; }

        static Func<int, Player, GateCard>[] GateCtrs =
        [
            //Vol. 1 Gate Cards
            (x, y) => new LevelDown(x, y),
            (x, y) => new Peacemaker(x, y),
            (x, y) => new Warlock(x, y),
            (x, y) => new CheeringBattle(x, y),
            (x, y) => new Transform(x, y),

            //Vol. 2 Gate Cards
            (x, y) => new JokersWild(x, y),
            (x, y) => new PositiveDelta(x, y),
            (x, y) => new Aquamerge(x, y),
            (x, y) => new GrandSpirit(x, y),
            (x, y) => new Supernova(x, y),
            (x, y) => new Reloaded(x, y),
            (x, y) => new AdditionalTask(x, y),
            (x, y) => new QuicksandFreeze(x, y),
            (x, y) => new NegativeDelta(x, y),

            //Vol. 3 Gate Cards
            (x, y) => new ResonanceCircuit(x, y),
            (x, y) => new Shockwave(x, y),
            (x, y) => new DarkInvitation(x, y),
            (x, y) => new PowerSpike(x, y),
            (x, y) => new MindGhost(x, y),
            (x, y) => new Anastasis(x, y),

            //Vol. 3 EX Gate Cards
            (x, y) => new DirectOpposition(x, y),
            (x, y) => new WindForcement(x, y),
            (x, y) => new EnergyMerge(x, y),
            (x, y) => new DetonationZone(x, y),
        ];

        public static GateCard CreateCard(Player owner, int cID, int type)
        {
            return GateCtrs[type].Invoke(cID, owner);
        }

        public List<Bakugan[]> EnterOrder = new();

        private protected Game game;

        public int CardId { get; set; }

        public List<Bakugan> Bakugans { get; set; } = new();
        public Player Owner { get; set; }
        public (byte X, byte Y) Position = (255, 255);
        public bool AllowAnyPlayers = false;

        public bool BattleStarted = false;
        public bool IsBattleGoing { get => Freezing.Count == 0 && (Bakugans.Select(x => x.Owner.TeamId).Distinct().Count() > 1 || (Bakugans.Count >= 2 && Bakugans.Any(x => x.Frenzied))); }
        public bool BattleStarting { get => !BattleStarted && IsBattleGoing; }
        public bool BattleDeclaredOver = false;
        public bool BattleOver = false;
        public bool BattleEnding { get => BattleDeclaredOver && !BattleOver; }
        public bool IsFrozen { get => Freezing.Count != 0; }
        public bool AllowsThrows { get => ThrowBlocking.Count != 0; }
        public List<object> Freezing = new();
        public List<object> OpenBlocking = new();
        public List<object> ThrowBlocking = new();
        public List<object> MovingInEffectBlocking = new();
        public List<object> MovingAwayEffectBlocking = new();
        public bool OnField { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool Negated = false;

        protected int currentTarget;

        protected Selector[] CondTargetSelectors = [];
        protected Selector[] ResTargetSelectors = [];


        public void Freeze(object frozer)
        {
            Freezing.Add(frozer);

            Console.WriteLine(GetType().ToString() + " frozen");

            Console.WriteLine("Battles going: " + game.isBattleGoing.ToString());
            BattleStarted = false;
        }

        public void TryUnfreeze(object frozer)
        {
            Freezing.Remove(frozer);
            if (Freezing.Count == 0)
                Console.WriteLine(GetType().ToString() + " unfrozen");
            else
                Console.WriteLine(GetType().ToString() + " still frozen");
            BattleStarted = false;
        }

        protected readonly List<Bakugan> bakugansDefeatedThisBattle = [];
        public virtual void DetermineWinner()
        {
            bakugansDefeatedThisBattle.Clear();
            BattleOver = true;
            BattleStarted = false;

            foreach (Bakugan b in Bakugans)
            {
                b.JustEndedBattle = true;
            }

            List<List<Bakugan>> sides = [];

            for (int i = 0; i < game.TeamCount; i++)
                sides.Add([.. Bakugans.Where(x => !x.Frenzied && x.Owner.TeamId == i)]);

            foreach (Bakugan bakugan in Bakugans.Where(x => x.Frenzied))
                sides.Add([bakugan]);

            BattleOver = true;

            if (sides.Count < 2) return;

            int[] teamTotals = new int[sides.Count];
            for (int i = 0; i < sides.Count; i++) teamTotals[i] = sides[i].Sum(x => x.Power);

            int winnerPower = teamTotals.Max();
            List<int> sidesToDefeat = [];
            for (int i = 0; i < sides.Count; i++)
                if (teamTotals[i] < winnerPower) sides[i].ForEach(x =>
                {
                    x.JustEndedBattle = false;
                    bakugansDefeatedThisBattle.Add(x);
                    x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
                });

            List<List<Bakugan>> survivingSides = [.. sides.Where(x => x.Any(y => y.Position == this))];
            if (survivingSides.Count > 1)
            {
                Bakugan randomFirstBakugan = EnterOrder[0][new Random().Next(EnterOrder[0].Length)];
                for (int i = 0; i < survivingSides.Count; i++)
                    if (!survivingSides[i].Contains(randomFirstBakugan))
                        survivingSides[i].ForEach(x =>
                        {
                            x.JustEndedBattle = false;
                            bakugansDefeatedThisBattle.Add(x);
                            x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
                        });
            }

            game.BattlesToEnd.Add(this);
        }

        public virtual void Dispose()
        {
            BattleDeclaredOver = false;
            if (!IsBattleGoing)
            {
                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                {
                    b.JustEndedBattle = false;
                    b.MoveFromFieldToHand(EnterOrder);
                }

                IsOpen = false;
                OnField = false;
                Owner.GateDrop.Add(this);

                game.Field[Position.X, Position.Y] = null;

                game.ThrowEvent(EventBuilder.RemoveGate(this));
                game.ThrowEvent(EventBuilder.SendGateToDrop(this));
            }
            else game.NextStep();
        }

        public virtual void ToDrop()
        {
            IsOpen = false;
            OnField = false;
            Owner.GateDrop.Add(this);

            game.Field[Position.X, Position.Y] = null;

            game.ThrowEvent(EventBuilder.RemoveGate(this));
            game.ThrowEvent(EventBuilder.SendGateToDrop(this));
        }

        public void Set(byte posX, byte posY)
        {
            IsOpen = false;
            game.Field[posX, posY] = this;
            OnField = true;
            Owner.GateHand.Remove(this);
            Position = (posX, posY);
            game.ThrowEvent(EventBuilder.GateSet(this, false), Owner.Id);
            game.ThrowEvent(Owner.Id, EventBuilder.GateSet(this, true));
            game.GateSetList.Add(this);
            game.OnGateAdded(this, Owner.Id, posX, posY);
        }

        public void Discard()
        {
            if (Owner.GateHand.Contains(this))
                Owner.GateHand.Remove(this);
            game.ThrowEvent(new()
            {
                ["Type"] = "GateRemovedFromHand",
                ["CardType"] = TypeId,
                ["CID"] = CardId,
                ["Owner"] = Owner.Id
            });
        }

        public static void MultiSet(Game game, GateCard[] gateCards, (byte posX, byte posY)[] positions, byte[] setBy)
        {
            for (int i = 0; i < gateCards.Length; i++)
            {
                GateCard card = gateCards[i];
                card.IsOpen = false;
                card.OnField = true;
                card.Owner.GateHand.Remove(card);
                game.Field[positions[i].posX, positions[i].posY] = card;
                card.Position = positions[i];
                game.GateSetList.Add(card);
            }

            var settables = gateCards.Zip(setBy, (first, second) => (first, second)).ToArray();
            for (byte i = 0; i < game.PlayerCount; i++)
                game.ThrowEvent(i, EventBuilder.MultiGateSet(settables, i));

            for (int i = 0; i < gateCards.Length; i++)
                game.OnGateAdded(gateCards[i], setBy[i], positions[i].posX, positions[i].posY);
        }

        public virtual void Retract()
        {
            game.ThrowEvent(EventBuilder.GateRetracted(this, false), Owner.Id);
            game.ThrowEvent(Owner.Id, EventBuilder.GateRetracted(this, true));
            game.ThrowEvent(new JObject
            {
                ["Type"] = "GateAddedToHand",
                ["Owner"] = Owner.Id,
                ["Kind"] = (int)Kind,
                ["CardType"] = TypeId,
                ["CID"] = CardId
            });
            OnField = false;
            (byte posX, byte posY) = Position;
            game.Field[posX, posY] = null;
            Owner.GateHand.Add(this);
            Position = (255, 255);
            game.GateSetList.Remove(this);
            game.OnGateRemoved(this, Owner.Id, posX, posY);
        }

        public virtual void Open()
        {
            game.OnGateOpen(this);
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));
            SendCondTargetForSelection();
        }

        protected void SendCondTargetForSelection()
        {
            if (CondTargetSelectors.Length <= currentTarget) game.CheckChain(Owner, this);
            else if (CondTargetSelectors[currentTarget].Condition())
            {
                var currentSelector = CondTargetSelectors[currentTarget];
                if (currentSelector is BakuganSelector bakuganSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "B" => EventBuilder.AnyBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BH" => EventBuilder.HandBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BF" => EventBuilder.FieldBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BG" => EventBuilder.DropBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is GateSelector gateSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "G" => throw new NotImplementedException(),
                            "GF" => EventBuilder.FieldGateSelection(currentSelector.Message, TypeId, (int)Kind, game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GH" => throw new NotImplementedException(),
                            "GG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is AbilitySelector abilitySelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "A" => EventBuilder.AbilitySelection(currentSelector.Message, game.AbilityIndex.Where(abilitySelector.TargetValidator)),
                            "AF" => throw new NotImplementedException(),
                            "AH" => throw new NotImplementedException(),
                            "AG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is ActiveSelector activeSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                }
                else if (currentSelector is OptionSelector optionSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                }
                else if (currentSelector is YesNoSelector yesNoSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                }
                else if (currentSelector is GateSlotSelector slotSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.FieldSlotSelection(currentSelector.Message, TypeId, (int)Kind)
                        ));
                }
                else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MB" => EventBuilder.AnyMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBH" => EventBuilder.HandMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBF" => EventBuilder.FieldMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBG" => EventBuilder.DropMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is MultiGateSlotSelector multiSlotSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.MultiFieldSlotSelection(currentSelector.Message, TypeId, (int)Kind, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                        ));
                }
                else
                {
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
                }
                game.OnAnswer[game.Players.First(currentSelector.ForPlayer).Id] = AcceptCondTarget;
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
                bakuganSelector.SelectedBakugan = game.BakuganIndex[(int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
            else if (currentSelector is GateSelector gateSelector)
                gateSelector.SelectedGate = game.GateIndex[(int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
            else if (currentSelector is AbilitySelector abilitySelector)
                abilitySelector.SelectedAbility = game.AbilityIndex[(int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["ability"]];
            else if (currentSelector is ActiveSelector activeSelector)
                activeSelector.SelectedActive = game.ActiveZone.First(x => x.EffectId == (int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
            else if (currentSelector is YesNoSelector yesNoSelector)
                yesNoSelector.IsYes = (bool)game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
            else if (currentSelector is OptionSelector optionSelector)
                optionSelector.SelectedOption = (int)game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
            else if (currentSelector is GateSlotSelector slotSelector)
                slotSelector.SelectedSlot = ((int)game.PlayerAnswers[Owner.Id]!["array"][0]["posX"], (int)game.PlayerAnswers[Owner.Id]!["array"][0]["posY"]);
            else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
            {
                JArray bakuganIds = game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => game.BakuganIndex[(int)x])];
            }
            else if (currentSelector is MultiGateSlotSelector multiSlotSelector)
            {
                JArray slots = game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["slots"];
                multiSlotSelector.SelectedSlots = [.. slots.Select(x => ((int)(x as JArray)![0], (int)(x as JArray)![1]))];
            }
            else
            {
                throw new NotImplementedException();
            }
            currentTarget++;
            SendCondTargetForSelection();
        }

        public virtual void Resolve()
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
            while (!ResTargetSelectors[currentTarget].HasValidTargets(game))
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
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "B" => EventBuilder.AnyBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BH" => EventBuilder.HandBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BF" => EventBuilder.FieldBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BG" => EventBuilder.DropBakuganSelection(currentSelector.Message, TypeId, (int)Kind, game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is GateSelector gateSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "G" => throw new NotImplementedException(),
                            "GF" => EventBuilder.FieldGateSelection(currentSelector.Message, TypeId, (int)Kind, game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GH" => throw new NotImplementedException(),
                            "GG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is AbilitySelector abilitySelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "A" => EventBuilder.AbilitySelection(currentSelector.Message, game.AbilityIndex.Where(abilitySelector.TargetValidator)),
                            "AF" => throw new NotImplementedException(),
                            "AH" => throw new NotImplementedException(),
                            "AG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else if (currentSelector is ActiveSelector activeSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                }
                else if (currentSelector is YesNoSelector yesNoSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                }
                else if (currentSelector is OptionSelector optionSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                }
                else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
                {
                    game.ThrowEvent(game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MB" => EventBuilder.AnyMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBH" => EventBuilder.HandMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBF" => EventBuilder.FieldMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBG" => EventBuilder.DropMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                }
                else
                {
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
                }
                game.OnAnswer[game.Players.First(currentSelector.ForPlayer).Id] = AcceptResTarget;
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
                bakuganSelector.SelectedBakugan = game.BakuganIndex[(int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
            else if (currentSelector is GateSelector gateSelector)
                gateSelector.SelectedGate = game.GateIndex[(int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
            else if (currentSelector is AbilitySelector abilitySelector)
            {
                //currently unused
                throw new NotImplementedException();
            }
            else if (currentSelector is ActiveSelector activeSelector)
                activeSelector.SelectedActive = game.ActiveZone.First(x => x.EffectId == (int)game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
            else if (currentSelector is YesNoSelector yesNoSelector)
                yesNoSelector.IsYes = (bool)game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
            else if (currentSelector is OptionSelector optionSelector)
                optionSelector.SelectedOption = (int)game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
            else if (currentSelector is MultiBakuganSelector multiBakuganSelector)
            {
                JArray bakuganIds = game.PlayerAnswers[game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => game.BakuganIndex[(int)x])];
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
            if (!Negated)
                TriggerEffect();

            game.ChainStep();
        }

        public virtual void TriggerEffect()
        { }

        public virtual bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !Negated && OnField && (IsBattleGoing || (game.Targets is not null && Bakugans.Any(x => game.Targets.Contains(x)))) && !IsOpen;

        public virtual int TypeId =>
            throw new NotImplementedException();

        public int EffectId { get; set; }

        public virtual CardKind Kind { get; } = CardKind.CommandGate;

        public bool IsAdjacent(GateCard card)
        {
            return AreAdjacent(this, card);
        }

        public static bool AreAdjacent(GateCard card1, GateCard card2)
        {
            if (!card1.OnField || !card2.OnField) return false;
            int DX = Math.Abs(card1.Position.X - card2.Position.X);
            int DY = Math.Abs(card1.Position.Y - card2.Position.Y);
            return (DX + DY) == 1;
        }

        public bool IsAdjacentVertically(GateCard card)
        {
            if (!OnField || !card.OnField) return false;
            return (card.Position.Y - Position.Y) == 0 && Math.Abs(card.Position.X - Position.X) == 1;
        }

        public bool IsAdjacentHorizontally(GateCard card)
        {
            if (!OnField || !card.OnField) return false;
            return (card.Position.X - Position.X) == 0 && Math.Abs(card.Position.Y - Position.Y) == 1;
        }

        public bool IsDiagonal(GateCard otherCard)
        {
            return Math.Abs(otherCard.Position.X - Position.X) == 1 && Math.Abs(otherCard.Position.Y - Position.Y) == 1;
        }

        public virtual void Negate(bool asCounter = false)
        {
            Negated = true;
            IsOpen = false;

            game.ThrowEvent(EventBuilder.GateNegated(this));
        }

        public void TransformFrom(GateCard source, bool conceded, params int[] revealTo)
        {
            Bakugans = source.Bakugans;
            EnterOrder = source.EnterOrder;
            Freezing = source.Freezing;
            OnField = source.OnField;
            Position = source.Position;
            BattleStarted = source.BattleStarted;
            BattleDeclaredOver = source.BattleDeclaredOver;
            BattleOver = source.BattleOver;
            ThrowBlocking = source.ThrowBlocking;
            MovingInEffectBlocking = source.MovingInEffectBlocking;
            MovingAwayEffectBlocking = source.MovingAwayEffectBlocking;

            if (!conceded)
                game.ThrowEvent(new JObject
                {
                    ["Type"] = "GateTransformed",
                    ["FromId"] = source.CardId,
                    ["FromPosX"] = source.Position.X,
                    ["FromPosY"] = source.Position.Y,
                    ["ToId"] = CardId,
                    ["ToKind"] = (int)Kind,
                    ["ToType"] = TypeId,
                    ["ToOwner"] = Owner.Id
                });
            else
            {
                foreach (var player in game.Players)
                    game.ThrowEvent(player.Id, new JObject
                    {
                        ["Type"] = "GateTransformed",
                        ["FromId"] = source.CardId,
                        ["FromPosX"] = source.Position.X,
                        ["FromPosY"] = source.Position.Y,
                        ["ToId"] = CardId,
                        ["ToKind"] = revealTo.Contains(player.Id) ? (int)Kind : -1,
                        ["ToType"] = revealTo.Contains(player.Id) ? TypeId : -2,
                        ["ToOwner"] = Owner.Id
                    });
            }
        }
    }
}
