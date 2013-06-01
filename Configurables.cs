﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    class Configurables
    {
        public const int NUMBER_OF_PLAYER_SHOTS_ALLOWED = 3;
        public const int SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP = 1000;
        public const bool ALLOW_ARBITRARY_SCORE_INCREASES = true;
        /// <summary>
        /// How many cycles the wave setup goes through before it returns to that of Wave 1.
        /// </summary>
        public const int INVADER_TURNOVER_INTERVAL = 3;
    }
}
