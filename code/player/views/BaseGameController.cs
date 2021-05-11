﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public abstract class BaseGameController : NetworkClass
	{
		public virtual void Tick( Player controller ) { }
	}
}
