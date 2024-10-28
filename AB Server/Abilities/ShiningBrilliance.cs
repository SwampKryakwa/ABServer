using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    //internal class ShiningBrillianceEffect
    //{
    //    public int TypeId { get; }
    //    Bakugan User;
    //    Game game;


    //    public Player Owner { get => User.Owner; } bool IsCopy;

    //    public ShiningBrillianceEffect(Bakugan user, Game game, int typeID, bool IsCopy)
    //    {
    //        User = user;
    //        this.game = game;
    //        user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
    //        TypeId = typeID;
    //    }

    //    public void Activate()
    //    {
    //        for (int i = 0; i < game.NewEvents.Length; i++)
    //        {
    //            game.NewEvents[i].Add(new()
    //            {
    //                { "Type", "AbilityActivateEffect" },
    //                { "Card", 12 },
    //                { "UserID", User.BID },
    //                { "User", new JObject {
    //                    { "Type", (int)User.Type },
    //                    { "Attribute", (int)User.Attribute },
    //                    { "Tretment", (int)User.Treatment },
    //                    { "Power", User.Power }
    //                }}
    //            });
    //        }

    //        foreach (Bakugan b in game.BakuganIndex.Where(x => x.OnField() && x.Owner == User.Owner && x.Attribute == Attribute.Lumina))
    //        {
    //            b.PermaBoost(50, this);
    //            User.affectingEffects.Add(this);
    //        }
    //    }
    //}

    //internal class ShiningBrilliance : AbilityCard, IAbilityCard
    //{
    //    public ShiningBrilliance(int cID, Player owner)
    //    {
    //        CardId = cID;
    //        Owner = owner;
    //        Game = owner.game;
    //    }

    //    public new void Resolve()
    //    {
    //        if (!counterNegated)
    //            new ShiningBrillianceEffect(User, Game, TypeId, IsCopy).Activate();

    //        Dispose();
    //    }

    //    public bool IsActivateableFusion(Bakugan user) =>
    //        user.OnField() && user.Attribute == Attribute.Lumina;

    //     = 12;
    //}
}
