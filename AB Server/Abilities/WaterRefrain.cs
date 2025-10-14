using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class WaterRefrain(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
            new WaterRefrainMarker(User, IsCopy).Activate();

    public override bool UserValidator(Bakugan user) =>
        Game.Players[Game.TurnPlayer].TeamId != Owner.TeamId && user.IsAttribute(Attribute.Aqua) && user.OnField();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.TurnStart;

    [ModuleInitializer]
    internal static void Init() => Register(4, CardKind.NormalAbility, (cID, owner) => new WaterRefrain(cID, owner, 4));
}


internal class WaterRefrainMarker(Bakugan user, bool isCopy) : IActive
{
    public int TypeId { get => 4; }
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public Bakugan User { get; set; } = user;
    Game game = user.Game;
    int turnsPassed = 0;

    public Player Owner { get; set; } = user.Owner;
    public CardKind Kind { get; } = CardKind.NormalAbility;

    public void Activate()
    {
        int team = User.Owner.TeamId;
        game.ActiveZone.Add(this);


        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));
        game.Players.Where(x => x.TeamId != Owner.TeamId).ToList().ForEach(p => p.RedAbilityBlockers.Add(this));

        game.OnTurnEnd += CheckEffectOver;

        User.AffectingEffects.Add(this);
    }

    public void CheckEffectOver()
    {
        if (turnsPassed++ == 1)
            CeaseMarker();
    }

    public void Negate(bool asCounter) => CeaseMarker();

    void CeaseMarker()
    {
        game.ActiveZone.Remove(this);
        Array.ForEach(game.Players, x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
        game.OnTurnEnd -= CheckEffectOver;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}
