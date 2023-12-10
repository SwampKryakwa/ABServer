﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class QuartetBattle : GateCard, IGateCard
    {
        public QuartetBattle(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CID = cID;
        }

        public new int GetTypeID()
        {
            return 2;
        }

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Open()
        {
            IsOpen = true;
            if (Bakugans.Count < 3)
                Freeze(this);

            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public new void Remove()
        {
            IsOpen = false;
            TryUnfreeze(this);

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, int pos)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }

        public void OnBakuganStands(Bakugan target, ushort owner, int pos)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }
    }
}
