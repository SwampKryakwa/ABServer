using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AB_Server
{
    internal class GraveBakugan : BakuganContainer
    {
        public Player Player;

        public List<Bakugan> Bakugans { get; } = new();

        public GraveBakugan(Player player)
        {
            Player = player;
        }
    }

    internal class Player : BakuganContainer
    {

        public ushort ID;
        public ushort SideID = new();
        public string DisplayName;

        public List<Bakugan> Bakugans { get; } = new();
        public List<IAbilityCard> AbilityHand = new();
        public List<IGateCard> GateHand = new();

        public GraveBakugan BakuganGrave;
        public List<IAbilityCard> AbilityGrave = new();
        public List<IGateCard> GateGrave = new();

        public List<Bakugan> BakuganOwned = new();

        public bool HasSetGate;
        public bool HasThrownBakugan;
        public bool HasSkippedTurn;
        public bool HasUsedFusion = false;
        public bool HasUsedCounter = false;

        public short hp = 3;

        public Game game;

        public Player(ushort id, ushort sideID, Game game, string displayName)
        {
            ID = id;
            SideID = sideID;
            this.game = game;
            BakuganGrave = new(this);
            DisplayName = displayName;
        }

        public static Player FromJson(ushort id, ushort sideID, JObject deck, Game game, string displayName)
        {
            Player player = new(id, sideID, game, displayName);

            //player.DisplayName = deck["dispName"].ToString();

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
                IAbilityCard abi = AbilityCard.CreateCard(player, game.AbilityIndex.Count, a);
                player.AbilityHand.Add(abi);
                game.AbilityIndex.Add(abi);
            }

            foreach (JObject g in deck["gates"])
            {
                IGateCard gate;
                if ((int)g["Type"] == 0)
                    gate = new NormalGate((Attribute)(int)g["Attribute"], (short)g["Power"], game.GateIndex.Count, game, player);
                else if ((int)g["Type"] == 4)
                    gate = new AttributeHazard(game.GateIndex.Count, player, (Attribute)(int)g["Attribute"]);
                else
                    gate = GateCard.CreateCard(player, game.GateIndex.Count, (int)g["Type"]);

                player.GateHand.Add(gate);
                game.GateIndex.Add(gate);
            }

            return player;
        }

        public bool HasThrowableBakugan()
        {
            return Bakugans.Any() && game.Field.Cast<GateCard>().Any(x => !(x?.DisallowedPlayers[ID] == true)) && !HasThrownBakugan;
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
            foreach (IGateCard g in game.Field)
            {
                if (g != null)
                    if (g.IsOpenable() && g.Owner == this)
                        return true;
            }
            return false;
        }

        public List<Bakugan> ThrowableBakugan()
        {
            return Bakugans;
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
            List<IGateCard> openableGates = new();
            foreach (IGateCard g in game.Field)
            {
                if (g != null)
                    if (g.IsOpenable() && g.Owner == this)
                        openableGates.Add(g);
            }
            return openableGates;
        }

        public bool CanEndTurn()
        {
            return !game.isFightGoing && (HasThrownBakugan | !HasSkippedTurn);
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
