﻿using System;

namespace Utils
{
    public static class Hashkey
    {
        public static ulong rand64(this Random rnd)
        {
            long r = rnd.Next();
            r <<= 31;
            r |= rnd.Next();
            r <<= 31;
            r |= rnd.Next();
            return (ulong) r;
        }
    }
}
