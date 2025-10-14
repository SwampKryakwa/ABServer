using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class FrenziedFlames(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
{
    public override void TriggerEffect() =>
        new FrenziedFlamesMarker(User, IsCopy).Activate();

    public override bool UserValidator(Bakugan user) =>
        user.JustEndedBattle && !user.BattleEndedInDraw && user.Position is GateCard posGate && posGate.BattleOver && user.IsAttribute(Attribute.Nova);

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(38, CardKind.NormalAbility, (cID, owner) => new FrenziedFlames(cID, owner, 38));
}


internal class FrenziedFlamesMarker(Bakugan user, bool IsCopy) : IActive
{
    public int TypeId { get => 38; }
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public Bakugan User { get; set; } = user;
    Game game = user.Game;
    public Player Owner { get; set; } = user.Owner;
    public CardKind Kind { get; } = CardKind.NormalAbility;
    Dictionary<Bakugan, Boost> currentBoosts = [];

    public void Activate()
    {
        int team = User.Owner.TeamId;
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        foreach (var bakugan in Owner.BakuganOwned.Where(x => x != User))
        {
            var boost = new Boost(200);
            bakugan.ContinuousBoost(boost, this);
            currentBoosts.Add(bakugan, boost);
        }

        game.OnTurnEnd += CheckEffectOver;
    }

    public void CheckEffectOver()
    {
        if (game.TurnPlayer == Owner.PlayerId)
            CeaseMarker();
    }

    public void Negate(bool asCounter) =>
        CeaseMarker();

    void CeaseMarker()
    {
        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        foreach ((var bakugan, var boost) in currentBoosts)
            bakugan.RemoveContinuousBoost(boost, this);

        game.OnTurnEnd -= CheckEffectOver;
    }
}
