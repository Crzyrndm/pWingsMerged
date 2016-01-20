using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWings
{
    public abstract class ProceduralControl : ProceduralWing
    {
        [KSPField]
        public float ctrlFraction = 1f;

        public const float costDensityControl = 6500f;

        /// <summary>
        /// control surfaces cant carry fuel
        /// </summary>
        public override bool canBeFueled
        {
            get
            {
                return false;
            }
        }

        public override float updateCost()
        {
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - ctrlFraction) + costDensityControl * ctrlFraction), 1);
        }
    }
}
