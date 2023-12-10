using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class AttributeHazard : GateCard, IGateCard
    {
        Attribute attribute;
        public AttributeHazard(int cID, Player owner, Attribute attribute)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CID = cID;
            this.attribute = attribute;
        }

        public new int GetTypeID()
        {
            return 4;
        }

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this)))
            {
                b.affectingEffects.Remove(this);
                b.Attribute = b.BaseAttribute;
            }

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Set(int pos)
        {
            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            base.Set(pos);
        }

        public void Trigger()
        {
            if (!IsOpen && !Negated) Open();
        }

        public new void Open()
        {
            IsOpen = true;
            Bakugans[0].affectingEffects.Add(this);
            Bakugans[0].Attribute = attribute;

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;

            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public new void Remove()
        {
            IsOpen = false;
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.affectingEffects.Contains(this)))
            {
                b.affectingEffects.Remove(this);
                b.Attribute = b.BaseAttribute;
            }

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, int pos)
        {
            if (pos == Position) Trigger();
        }

        public void OnBakuganStands(Bakugan target, ushort owner, int pos)
        {
            if (pos == Position) Trigger();
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if(target.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                target.Attribute = target.BaseAttribute;
            }
        }
    }
}
