using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server
{
    internal class NewGame
    {
        //Player data
        public Dictionary<long, int> UidToPid = [];
        public byte PlayerCount;
        public Player[] Players;
        public List<long> Spectators = [];

        //Indexes
        public List<Bakugan> BakuganIndex = [];
        public List<GateCard> GateIndex = [];
        public List<AbilityCard> AbilityIndex = [];

        //Event containers
        public List<JObject>[] NewEvents;
        public Dictionary<long, List<JObject>> SpectatorEvents = [];

        //Game field
        public GateCard?[,] Field = new GateCard[2, 3];
        public List<GateCard> GateSetList = [];
        public List<IActive> ActiveZone = [];
        public List<IChainable> CardChain = [];

        //Game state
        public byte TurnPlayer;
        public byte ActivePlayer;
        public bool isBattleGoing { get => GateIndex.Any(x => x.OnField && x.ActiveBattle); }
        public ActivationWindow CurrentWindow = ActivationWindow.Normal;

        //Communication with the players
        public dynamic?[] PlayerAnswers;
        public Action[] OnAnswer;

        //Other data
        byte playersCreated = 0;

        //All the event types in the game
        public delegate void BakuganBoostedEffect(Bakugan target, Boost boost, object source);
        public delegate void BakuganPowerResetEffect(Bakugan bakugan);
        public delegate void BakuganMovedEffect(Bakugan target, IBakuganContainer pos);
        public delegate void BakuganReturnedEffect(Bakugan target, byte owner);
        public delegate void BakuganDestroyedEffect(Bakugan target, byte owner);
        public delegate void BakuganRevivedEffect(Bakugan target, byte owner);
        public delegate void BakuganThrownEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganAddedEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganPlacedFromGraveEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void GateAddedEffect(GateCard target, byte owner, params byte[] pos);
        public delegate void GateRemovedEffect(GateCard target, byte owner, params byte[] pos);
        public delegate void GateOpenEffect(GateCard target);
        public delegate void BattlesStartedEffect();
        public delegate void BattlesOverEffect();
        public delegate void TurnStartedEffect();
        public delegate void TurnAboutToEndEffect();
        public delegate void TurnEndEffect();

        //All the events in the game
        public event BakuganBoostedEffect BakuganBoosted;
        public event BakuganPowerResetEffect BakuganPowerReset;
        public event BakuganMovedEffect BakuganMoved;
        public event BakuganReturnedEffect BakuganReturned;
        public event BakuganDestroyedEffect BakuganDestroyed;
        public event BakuganRevivedEffect BakuganRevived;
        public event BakuganThrownEffect BakuganThrown;
        public event BakuganAddedEffect BakuganAdded;
        public event BakuganPlacedFromGraveEffect BakuganPlacedFromGrave;
        public event GateAddedEffect GateAdded;
        public event GateRemovedEffect GateRemoved;
        public event GateOpenEffect GateOpen;
        public event BattlesStartedEffect BattlesStarted;
        public event BattlesOverEffect BattlesOver;
        public event TurnStartedEffect TurnStarted;
        public event TurnAboutToEndEffect TurnAboutToEnd;
        public event TurnEndEffect TurnEnd;

        //Access to the game's events
        public void OnBakuganBoosted(Bakugan target, Boost boost, object source) =>
            BakuganBoosted?.Invoke(target, boost, source);
        public void OnBakuganMoved(Bakugan target, IBakuganContainer pos) =>
            BakuganMoved?.Invoke(target, pos);
        public void OnBakuganAdded(Bakugan target, byte owner, IBakuganContainer pos) =>
            BakuganAdded?.Invoke(target, owner, pos);
        public void OnBakuganThrown(Bakugan target, byte owner, IBakuganContainer pos)
        {
            BakuganAdded?.Invoke(target, owner, pos);
            BakuganThrown?.Invoke(target, owner, pos);
        }
        public void OnBakuganPlacedFromGrave(Bakugan target, byte owner, IBakuganContainer pos)
        {
            BakuganPlacedFromGrave?.Invoke(target, owner, pos);
            BakuganAdded?.Invoke(target, owner, pos);
        }
        public void OnBakuganReturned(Bakugan target, byte owner) =>
            BakuganReturned?.Invoke(target, owner);
        public void OnBakuganDestroyed(Bakugan target, byte owner) =>
            BakuganDestroyed?.Invoke(target, owner);
        public void OnBakuganRevived(Bakugan target, byte owner) =>
            BakuganRevived?.Invoke(target, owner);
        public void OnGateAdded(GateCard target, byte owner, params byte[] pos) =>
            GateAdded?.Invoke(target, owner, pos);
        public void OnGateRemoved(GateCard target, byte owner, params byte[] pos) =>
            GateRemoved?.Invoke(target, owner, pos);
        public void OnGateOpen(GateCard target) =>
            GateOpen?.Invoke(target);

        public NewGame(byte playerCount)
        {
            PlayerCount = playerCount;
            Players = new Player[playerCount];
            NewEvents = new List<JObject>[playerCount];
            PlayerAnswers = new JObject[playerCount];
            OnAnswer = new Action[playerCount];
            for (byte i = 0; i < playerCount; i++)
            {
                NewEvents[i] = new List<JObject>();
            }
        }

        public int CreatePlayer(string userName, byte team, long uuid)
        {
            Players[playersCreated] = new Player(playersCreated, team, userName);
            UidToPid.Add(uuid, playersCreated);
            return playersCreated++;
        }

        public void RegisterPlayer()
    }
}
