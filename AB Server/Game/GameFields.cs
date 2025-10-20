
using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server;

internal partial class Game
{

    //Player data
    public Dictionary<long, int> UidToPid = [];
    public byte PlayerCount;
    public byte TeamCount;
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
    public GateCard?[,] Field = new GateCard[4, 5];
    public List<GateCard> GateSetList = [];
    public List<IActive> ActiveZone = [];
    public Stack<IChainable> CardChain = [];

    //Game state
    bool Over = false;
    public int turnNumber = 0;
    public byte TurnPlayer;
    public byte ActivePlayer;
    public bool IsBattleGoing { get => GateIndex.Any(x => x.OnField && x.IsBattleGoing); }
    public ActivationWindow CurrentWindow = ActivationWindow.Normal;
    public readonly List<GateCard> BattlesToEnd = [];
    public readonly List<GateCard> AutoGatesToOpen = [];
    readonly List<Player> playersPassed = [];
    public byte PlayersLeft = 0;

    //Communication with the players
    public dynamic?[] PlayerAnswers;
    public Action[] OnAnswer;
    public Action[] OnCancel;

    //Long-range battles stuff
    public bool LongRangeBattleGoing;
    public Bakugan? Attacker;
    public Bakugan[]? Targets;
    public Action? OnLongRangeBattleOver;

    //Game flow
    public Action NextStep;

    //Other data
    byte[] playersCreatedInTeam;
    byte playersRegistered = 0;
    public int NextEffectId = 0;
    public bool DoNotMakeStep = false;
}