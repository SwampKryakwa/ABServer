﻿using AB_Server.Abilities;
using AB_Server.Abilities.Correlations;
using AB_Server.Gates;

namespace AB_Server
{
    internal class GraveBakugan : IBakuganContainer
    {
        public Player Player;

        public List<Bakugan> Bakugans { get; } = new();

        public GraveBakugan(Player player)
        {
            Player = player;
        }
    }

    internal class Player : IBakuganContainer
    {

        public byte Id;
        public byte SideID = new();
        public string DisplayName;
        public byte Avatar;
        public List<object> AbilityBlockers = new();

        public List<Bakugan> Bakugans { get; } = new();
        public List<AbilityCard> AbilityHand = new();
        public List<GateCard> GateHand = new();

        public GraveBakugan BakuganGrave;
        public List<AbilityCard> AbilityGrave = new();
        public List<GateCard> GateGrave = new();

        public List<Bakugan> BakuganOwned = new();

        public bool HadSetGate;
        public bool HadThrownBakugan;
        public bool HadSkippedTurn { get; set; }
        public bool HadUsedCounter = false;

        public short hp = 3;

        public Game game;

        public byte playerColor;

        public Player(byte id, byte sideID, Game game, string displayName, byte avatar)
        {
            Id = id;
            SideID = sideID;
            this.game = game;
            BakuganGrave = new(this);
            DisplayName = displayName;
            Avatar = avatar;
        }

        public static Player FromJson(byte id, byte sideID, dynamic deck, Game game, string displayName, byte avatar)
        {
            Player player = new(id, sideID, game, displayName, avatar);

            //player.DisplayName = deck["dispName"].ToString();

            if (deck["deck_color"] != null)
                player.playerColor = deck["deck_color"];
            else
                player.playerColor = 0;

            foreach (dynamic b in deck["bakugans"])
            {
                int type = (int)b["Type"];
                short power = (short)b["Power"];
                int attr = (int)b["Attribute"];
                int treatment = (int)b["Treatment"];

                Bakugan bak = new((BakuganType)type, power, (Attribute)attr, (Treatment)treatment, player, game, game.BakuganIndex.Count);
                game.BakuganIndex.Add(bak);
                player.Bakugans.Add(bak);
                player.BakuganOwned.Add(bak);
            }

            foreach (int a in deck["abilities"])
            {
                AbilityCard abi = AbilityCard.CreateCard(player, game.AbilityIndex.Count, a);
                player.AbilityHand.Add(abi);
                game.AbilityIndex.Add(abi);
            }

            FusionAbility fusion = FusionAbility.FusionCtrs[deck["bakugans"][0]["Type"]].Invoke(game.AbilityIndex.Count, player);
            player.AbilityHand.Add(fusion);
            game.AbilityIndex.Add(fusion);

            player.BakuganOwned[0].IsPartner = true;
            HashSet<Attribute> combinedAttributes = new(player.BakuganOwned.Select(x => x.Attribute));
            if (combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Nova, Attribute.Lumina, Attribute.Aqua }) ||
                   combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Zephyros, Attribute.Subterra, Attribute.Darkon }))
            {
                TripleNode tripleNode = new(game.AbilityIndex.Count, player);
                game.AbilityIndex.Add(tripleNode);
                player.AbilityHand.Add(tripleNode);
            }
            else if (combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Nova, Attribute.Darkon }) ||
                   combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Aqua, Attribute.Subterra }) ||
                   combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Zephyros, Attribute.Lumina }))
            {
                DiagonalCorrelation diagonalCorrelation = new(game.AbilityIndex.Count, player);
                game.AbilityIndex.Add(diagonalCorrelation);
                player.AbilityHand.Add(diagonalCorrelation);
            }
            else
            {
                AdjacentCorrelation adjacentCorrelation = new(game.AbilityIndex.Count, player);
                game.AbilityIndex.Add(adjacentCorrelation);
                player.AbilityHand.Add(adjacentCorrelation);
            }

            foreach (dynamic g in deck["gates"])
            {
                GateCard gate;
                gate = GateCard.CreateCard(player, game.GateIndex.Count, (int)g["Type"]);

                player.GateHand.Add(gate);
                game.GateIndex.Add(gate);
            }

            return player;
        }

        public bool HasThrowableBakugan() =>
            Bakugans.Count != 0 && game.Field.Cast<GateCard>().Any(x => x != null) && !HadThrownBakugan;

        public bool HasActivateableAbilities() =>
            AbilityHand.Any(x => x.IsActivateable());

        public bool HasActivateableFusionAbilities(Bakugan user) =>
            AbilityHand.Any(x => x.IsActivateableByBakugan(user));

        public bool HasSettableGates() =>
            GateHand.Count != 0 && !HadSetGate;

        public bool HasOpenableGates()
        {
            foreach (GateCard? gate in game.Field)
            {
                if (gate is GateCard g)
                    if (g.IsOpenable() && g.Owner == this)
                        return true;
            }
            return false;
        }

        public List<Bakugan> ThrowableBakugan() =>
            Bakugans;

        public List<GateCard> SettableGates() =>
            GateHand;

        //public List<AbilityCard> ActivateableAbilities()
        //{
        //    return AbilityHand.Where(x => x.IsActivateable()).ToList();
        //}

        public List<GateCard> OpenableGates()
        {
            List<GateCard> openableGates = new();
            foreach (GateCard? gate in game.Field)
            {
                if (gate is GateCard g)
                    if (g.IsOpenable() && g.Owner == this)
                        openableGates.Add(g);
            }
            return openableGates;
        }

        public bool CanEndTurn()
        {
            return !game.isBattleGoing && (Bakugans.Count == 0 || HadThrownBakugan || !HadSkippedTurn);
        }

        public bool HasBattlingBakugan() =>
            BakuganOwned.Any(x => x.InBattle);
    }
}
