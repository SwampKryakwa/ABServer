﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    //internal class SerpentSqueezeEffect
    //{
    //    public int TypeId { get; }
    //    public Bakugan User;
    //    Bakugan target;
    //    Game game;
    //    int change;


    //    public Player Owner { get => User.Owner; }

    //    public SerpentSqueezeEffect(Bakugan user, Bakugan target, Game game, int typeID)
    //    {
    //        User = user;
    //        this.game = game;
    //        this.target = target;
    //        user.UsedAbilityThisTurn = true;
    //        TypeId = typeID;
    //    }

    //    public void Activate()
    //    {
    //        int team = User.Owner.SideID;

    //        for (int i = 0; i < game.NewEvents.Length; i++)
    //        {
    //            game.NewEvents[i].Add(new()
    //            {
    //                { "Type", "AbilityActivateEffect" },
    //                { "Card", 21 },
    //                { "UserID", User.BID },
    //                { "User", new JObject {
    //                    { "Type", (int)User.Type },
    //                    { "Attribute", (int)User.Attribute },
    //                    { "Tretment", (int)User.Treatment },
    //                    { "Power", User.Power }
    //                }}
    //            });
    //        }
    //        change = User.SwitchPowers(target);

    //        game.BakuganReturned += FieldLeaveTurnover;
    //        game.BakuganDestroyed += FieldLeaveTurnover;
    //        game.BakuganPowerReset += ResetTurnover;

    //        User.affectingEffects.Add(this);
    //    }

    //    //remove when goes to hand
    //    //remove when goes to grave
    //    public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
    //    {
    //        if (leaver == User && User.affectingEffects.Contains(this))
    //        {
    //            User.affectingEffects.Remove(this);
    //            game.BakuganReturned -= FieldLeaveTurnover;
    //            game.BakuganDestroyed -= FieldLeaveTurnover;
    //            game.BakuganPowerReset -= ResetTurnover;
    //        }
    //    }

    //    //remove when power reset
    //    public void ResetTurnover(Bakugan leaver)
    //    {
    //        if (leaver == User && User.affectingEffects.Contains(this))
    //        {
    //            User.affectingEffects.Remove(this);
    //            game.BakuganReturned -= FieldLeaveTurnover;
    //            game.BakuganDestroyed -= FieldLeaveTurnover;
    //            game.BakuganPowerReset -= ResetTurnover;
    //        }
    //    }
    //}

    //internal class SerpentSqueeze : AbilityCard, IAbilityCard
    //{
    //     = 21;

    //    public SerpentSqueeze(int cID, Player owner)
    //    {
    //        CardId = cID;
    //        Owner = owner;
    //        Game = owner.game;
    //    }

    //    private Bakugan target;

    //    public void Setup(bool asCounter)
    //    {
    //        IAbilityCard ability = this;
            
    //        Game.NewEvents[Owner.Id].Add(new JObject
    //        {
    //            { "Type", "StartSelection" },
    //            { "Count", 1 },
    //            { "Selections", new JArray {
    //                new JObject {
    //                    { "SelectionType", "BF" },
    //                    { "Message", "INFO_ABILITYUSER" },
    //                    { "Ability", TypeId },
    //                    { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
    //                        new JObject { { "Type", (int)x.Type },
    //                            { "Attribute", (int)x.Attribute },
    //                            { "Treatment", (int)x.Treatment },
    //                            { "Power", x.Power },
    //                            { "Owner", x.Owner.Id },
    //                            { "BID", x.BID } })) }
    //                }
    //            } }
    //        });

    //        Game.awaitingAnswers[Owner.Id] = Setup2;
    //    }

    //    public void SetupFusion(IAbilityCard parentCard, Bakugan user)
    //    {
    //        User = user;
            
    //        Game.NewEvents[Owner.Id].Add(new JObject
    //        {
    //            { "Type", "StartSelection" },
    //            { "Count", 1 },
    //            { "Selections", new JArray {
    //                new JObject {
    //                    { "SelectionType", "BF" },
    //                    { "Message", "INFO_ABILITYTARGET" },
    //                    { "Ability", TypeId },
    //                    { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x != User).Select(x =>
    //                        new JObject { { "Type", (int)x.Type },
    //                            { "Attribute", (int)x.Attribute },
    //                            { "Treatment", (int)x.Treatment },
    //                            { "Power", x.Power },
    //                            { "Owner", x.Owner.Id },
    //                            { "BID", x.BID } })) }
    //                }
    //            } }
    //        });

    //        Game.awaitingAnswers[Owner.Id] = Activate;
    //    }

    //    public void Setup2()
    //    {
    //        User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
    //        Game.NewEvents[Owner.Id].Add(new JObject
    //        {
    //            { "Type", "StartSelection" },
    //            { "Count", 1 },
    //            { "Selections", new JArray {
    //                new JObject {
    //                    { "SelectionType", "BF" },
    //                    { "Message", "INFO_ABILITYTARGET" },
    //                    { "Ability", TypeId },
    //                    { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x != User).Select(x =>
    //                        new JObject { { "Type", (int)x.Type },
    //                            { "Attribute", (int)x.Attribute },
    //                            { "Treatment", (int)x.Treatment },
    //                            { "Power", x.Power },
    //                            { "Owner", x.Owner.Id },
    //                            { "BID", x.BID } })) }
    //                }
    //            } }
    //        });

    //        Game.awaitingAnswers[Owner.Id] = Activate;
    //    }

    //    public void Activate()
    //    {
    //        target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

    //        Game.CheckChain(Owner, this, User);
    //    }

    //    public new void Resolve()
    //    {
    //        if (!counterNegated)
    //            new FireTornadoEffect(User, target, Game, 1).Activate();

    //        Dispose();
    //    }

    //    public bool IsActivateableFusion(Bakugan user) => user.InBattle && user.OnField() && user.Type == BakuganType.Serpent;
    //}
}
