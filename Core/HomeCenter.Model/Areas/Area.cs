﻿using System;
using System.Collections.Generic;
using System.Text;
using HomeCenter.ComponentModel;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Core.Services.DependencyInjection;

namespace HomeCenter.Core.ComponentModel.Areas
{
    public class Area : BaseObject
    {
        [Map] private List<Area> _areas { get; set; } = new List<Area>();
        private List<Component> _components { get; set; } = new List<Component>();

        public IReadOnlyCollection<Area> Areas => _areas.AsReadOnly();

        public void AddComponent(Component component)
        {
            _components.Add(component);
        }

        public override string ToString() => $"{Uid} with {_components.Count} components";
    }
}