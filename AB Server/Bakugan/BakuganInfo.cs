using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server;

internal partial class Bakugan
{
    //Relations
    public Game Game = game;
    public Player Owner = owner;

    //Bakugan info
    public int BID = BID;
    public BakuganType Type = type;
    public bool IsPartner = false;

    //Extra info
    public bool IsDummy = false;
}
