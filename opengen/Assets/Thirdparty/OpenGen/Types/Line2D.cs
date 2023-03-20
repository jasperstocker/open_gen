using System;
using UnityEngine;

namespace opengen.types
{
	[Serializable]
	public class Line2D
	{
		[SerializeField] private Vector2 _p0;
		[SerializeField] private Vector2 _p1;
		[SerializeField] private int _index;
	}
}