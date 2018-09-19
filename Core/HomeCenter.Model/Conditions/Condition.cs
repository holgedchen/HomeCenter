﻿using HomeCenter.Model.Core;
using System.Threading.Tasks;

namespace HomeCenter.Conditions
{
    public abstract class Condition : BaseObject, IValidable
    {
        public abstract Task<bool> Validate();

        public bool IsInverted { get; set; }
    }
}