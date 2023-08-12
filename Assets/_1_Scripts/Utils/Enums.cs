﻿using System;

namespace NomadsPlanet.Utils
{
    public enum LightType
    {
        Red,
        Yellow,
        Green
    }

    public enum LaneType
    {
        First,
        Second,
    }

    [Flags]
    public enum TrafficType
    {
        Left = 1,
        Right = 2,
        Forward = 4,
    }

    public enum CharacterType
    {
        Aqua,
        Cute,
        Green,
        Idol,
        Mint,
        Orange,
        Pink,
        Red,
    }

    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimeOut,
    }
}