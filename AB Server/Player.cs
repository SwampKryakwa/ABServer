using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

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
        public bool HadUsedFusion = false;
        public bool HadUsedCounter = false;

        public short hp = 3;

        public Game game;

        public Player(byte id, byte sideID, Game game, string displayName)
        {
            Id = id;
            SideID = sideID;
            this.game = game;
            BakuganGrave = new(this);
            DisplayName = displayName;
        }

        public static Player FromJson(byte id, byte sideID, dynamic deck, Game game, string displayName)
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
                AbilityCard abi = AbilityCard.CreateCard(player, game.AbilityIndex.Count, a);
                player.AbilityHand.Add(abi);
                game.AbilityIndex.Add(abi);
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
            AbilityHand.Any(x => x.IsActivateableFusion(user));

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
            Console.WriteLine($"Battle going: {game.isBattleGoing}");
            Console.WriteLine($"Bakugan count: {Bakugans.Count}");
            Console.WriteLine($"Had thrown Bakugan: {HadThrownBakugan}");
            Console.WriteLine($"Had skipped turn: {HadSkippedTurn}");
            Console.WriteLine($"Can end turn: {!game.isBattleGoing && (Bakugans.Count == 0 || HadThrownBakugan || !HadSkippedTurn)}");
            return !game.isBattleGoing && (Bakugans.Count == 0 || HadThrownBakugan || !HadSkippedTurn);
        }

        public bool CanEndBattle() =>
            game.BakuganIndex.Any(x => x.Owner == this && x.InBattle);

        /*public static Player FromJSON(JObject playerJson, byte playerID)
        {
            dynamic playerObject = playerJson;
            Player player = new Player(playerID);
            player.Bakugans = new List<Bakugan>();

            foreach (string card in playerObject["abilities"])
                player.Abilities.Add((AbilityCard)Activator.CreateInstance(Type.GetType("Advanced_Brawl.Abilities." + card)));

            foreach (JObject card in playerObject["gates"])
            {
                if (card["Type"].ToString() == "NormalGate")
                    player.Gates.Add(new NormalGate(Bakugan.nameToAttribute[card["attribute"].ToString()], (short)card["Power"]));
                else
                    player.Gates.Add((GateCard)Activator.CreateInstance(Type.GetType("Advanced_Brawl.Gates." + card.GetValue("Type").ToString())));
            }
            player.Gates.Cast<GateCard>().ToList().ForEach(x => x.Owner = playerID);
            foreach (dynamic bakugan in playerObject["bakugans"])
                player.Bakugans.Add(new Bakugan((string)bakugan["Type"], (short)bakugan["Power"], Bakugan.nameToAttribute[bakugan["attribute"].ToString()], Treatment.None, playerID));

            return player;
        }*/
    }
}
