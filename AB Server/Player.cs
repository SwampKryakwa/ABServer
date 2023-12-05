﻿using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AB_Server
{
    internal class Player
    {

        public ushort ID;
        public ushort SideID = new();
#pragma warning disable CS0649
        public string DisplayName;
#pragma warning restore CS0649

        public List<Bakugan> BakuganHand = new();
        public List<IAbilityCard> AbilityHand = new();
        public List<IGateCard> GateHand = new();

        public List<Bakugan> BakuganGrave = new();
        public List<IAbilityCard> AbilityGrave = new();
        public List<IGateCard> GateGrave = new();

        public List<Bakugan> BakuganOwned = new();

        public bool HasSetGate;
        public bool HasThrownBakugan;
        public bool HasSkippedTurn;

        public short hp = 3;

        public Game game;

        public Player(ushort id, ushort sideID, Game game)
        {
            ID = id;
            SideID = sideID;
            this.game = game;
        }

        public static Player FromJson(ushort id, ushort sideID, JObject deck, Game game)
        {
            Player player = new(id, sideID, game);

            //player.DisplayName = deck["dispName"].ToString();

            foreach (dynamic b in deck["bakugans"])
            {
                int type = (int)b["Type"];
                short power = (short)b["Power"];
                int attr = (int)b["Attribute"];
                int treatment = (int)b["Treatment"];

                Bakugan bak = new((BakuganType)type, power, (Attribute)attr, (Treatment)treatment, player, game, game.BakuganIndex.Count);
                game.BakuganIndex.Add(bak);
                player.BakuganHand.Add(bak);
                player.BakuganOwned.Add(bak);
            }

            foreach (int a in deck["abilities"])
            {
                IAbilityCard abi = AbilityCard.CreateCard(player, game.AbilityIndex.Count, a);
                player.AbilityHand.Add(abi);
                game.AbilityIndex.Add(abi);
            }

            foreach (JObject g in deck["gates"])
            {
                IGateCard gate;
                if ((int)g["Type"] != 0)
                    gate = GateCard.CreateCard(player, game.GateIndex.Count, (int)g["Type"]);
                else
                    gate = new NormalGate((Attribute)(int)g["Attribute"], (short)g["Power"], game.GateIndex.Count, game, player);

                player.GateHand.Add(gate);
                game.GateIndex.Add(gate);
            }

            return player;
        }

        public bool HasThrowableBakugan()
        {
            return BakuganHand.Any() && game.Field.Cast<GateCard>().Any(x => !(x?.DisallowedPlayers[ID] == true)) && !HasThrownBakugan;
        }

        public bool HasActivatableAbilities()
        {
            return AbilityHand.Any(x => x.IsActivateable());
        }

        public bool HasActivatableAbilities(bool asFusion)
        {
            return AbilityHand.Any(x => x.IsActivateable(asFusion));
        }

        public bool HasSettableGates()
        {
            return GateHand.Count != 0 && !HasSetGate;
        }

        public bool HasOpenableGates()
        {
            return game.Field.Cast<IGateCard>().Any(x => (x?.IsOpenable() == true) && x.Owner == this);
        }

        public List<Bakugan> ThrowableBakugan()
        {
            return BakuganHand;
        }

        public List<IGateCard> SettableGates()
        {
            return GateHand;
        }

        public List<IAbilityCard> ActivateableAbilities()
        {
            return AbilityHand.Where(x => x.IsActivateable()).ToList();
        }

        public List<IGateCard> OpenableGates()
        {
            return game.Field.Cast<IGateCard>().Where(x => (x?.IsOpenable() == true) && x.Owner == this).ToList();
        }

        public bool CanEndTurn()
        {
            return !game.isFightGoing && (HasThrownBakugan || !HasSkippedTurn);
        }

        public bool CanEndBattle()
        {
            return game.BakuganIndex.Any(x => x.Owner == this && x.InBattle);
        }

        /*public static Player FromJSON(JObject playerJson, ushort playerID)
        {
            dynamic playerObject = playerJson;
            Player player = new Player(playerID);
            player.Bakugans = new List<Bakugan>();

            foreach (string card in playerObject["abilities"])
                player.Abilities.Add((IAbilityCard)Activator.CreateInstance(Type.GetType("Advanced_Brawl.Abilities." + card)));

            foreach (JObject card in playerObject["gates"])
            {
                if (card["Type"].ToString() == "NormalGate")
                    player.Gates.Add(new NormalGate(Bakugan.nameToAttribute[card["attribute"].ToString()], (short)card["Power"]));
                else
                    player.Gates.Add((IGateCard)Activator.CreateInstance(Type.GetType("Advanced_Brawl.Gates." + card.GetValue("Type").ToString())));
            }
            player.Gates.Cast<GateCard>().ToList().ForEach(x => x.Owner = playerID);
            foreach (dynamic bakugan in playerObject["bakugans"])
                player.Bakugans.Add(new Bakugan((string)bakugan["Type"], (short)bakugan["Power"], Bakugan.nameToAttribute[bakugan["attribute"].ToString()], Treatment.None, playerID));

            return player;
        }*/
    }
}
