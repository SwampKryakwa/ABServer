using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    //internal class CinderCoilEffect
    //{
    //    public int TypeId { get; }
    //    Bakugan User;
    //    Game game;

    //    short boost = 0;

    //    IAbilityCard card;

    //    public Player Owner { get => User.Owner; }

    //    public CinderCoilEffect(Bakugan user, Game game, int typeID, IAbilityCard card)
    //    {
    //        this.User = user;
    //        this.game = game;
    //        user.UsedAbilityThisTurn = true;
    //        TypeId = typeID;
    //        this.card = card;
    //    }

    //    public void Activate()
    //    {
    //        for (int i = 0; i < game.NewEvents.Length; i++)
    //        {
    //            game.NewEvents[i].Add(new()
    //            {
    //                { "Type", "AbilityActivateEffect" },
    //                { "Card", TypeId },
    //                { "UserID", User.BID },
    //                { "User", new JObject {
    //                    { "Type", (int)User.Type },
    //                    { "Attribute", (int)User.Attribute },
    //                    { "Tretment", (int)User.Treatment },
    //                    { "Power", User.Power }
    //                }}
    //            });
    //        }

    //        game.BakuganReturned += FieldLeaveTurnover;
    //        game.BakuganDestroyed += EffectOver;
    //        game.BakuganPowerReset += Reset;

    //        game.BakuganBoosted += Trigger;
    //        User.affectingEffects.Add(this);
    //    }

    //    public void Trigger(Bakugan target, short boost, object source)
    //    {
    //        if (target == User && source.GetType() != typeof(CinderCoilEffect))
    //        {
    //            target.Boost(400, this);
    //            boost += 400;
    //        }
    //    }

    //    public void Reset(Bakugan target)
    //    {
    //        if (target == User)
    //            boost = 0;
    //    }

    //    public void EffectOver(Bakugan target, ushort owner)
    //    {
    //        if (target != User) return;
    //        game.BakuganReturned -= FieldLeaveTurnover;
    //        game.BakuganDestroyed -= EffectOver;

    //        game.BakuganBoosted -= Trigger;
    //        game.BakuganPowerReset -= Reset;
    //        User.affectingEffects.Remove(this);

    //        User.Boost((short)-boost, this);
    //    }

    //    //remove when goes to hand
    //    //remove when goes to grave
    //    public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
    //    {
    //        if (leaver == User && User.affectingEffects.Contains(this))
    //        {
    //            game.BakuganReturned -= FieldLeaveTurnover;
    //            game.BakuganDestroyed -= EffectOver;
    //        }
    //    }
    //}

    //internal class CinderCoil : AbilityCard, IAbilityCard
    //{

    //    public CinderCoil(int cID, Player owner)
    //    {
    //        CardId = cID;
    //        Owner = owner;
    //        Game = owner.game;
    //    }

    //    public new void Resolve()
    //    {
    //        if (!counterNegated)
    //            new CinderCoilEffect(User, Game, 0, this).Activate();

    //        Dispose();
    //    }

    //    public bool IsActivateableFusion(Bakugan user) =>
    //        user.OnField() && user.Type == BakuganType.Serpent && user.Attribute == Attribute.Nova;

    //     = 22;
    //}
}