﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoleculeMOTHardwareControl.Controls
{
    public abstract class GenericController : MarshalByRefObject
    {
        public GenericView view;

        public GenericController()
        {
            view = CreateControl();
            view.controller = this;
        }

        abstract protected GenericView CreateControl(); // Derived classes must implement this method to create the controls

        public virtual Dictionary<string, object> Report()
        {
            return null;
        }
    }
}
