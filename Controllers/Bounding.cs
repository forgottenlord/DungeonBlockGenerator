using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CursorBounds
{/*
	public class Bounding : MonoBehaviour
	{
		private bool _isBound = false;

		public bool isBound
		{
			get
			{
				return _isBound;
			}
			set
			{
				if (_isBound != value)
				{
					if (value)
						Binding.ClampCursor();
					else
						Binding.ReleaseCursor();

					_isBound = value;
				}
			}
		}

		private void Update()
		{
			if (Application.isEditor) return;

			if (Input.GetMouseButtonDown(0))
			{
				isBound = true;
			}

			if (Input.GetKeyDown(KeyCode.Escape) || !Application.isFocused)
			{
				isBound = false;
			}
		}
	}*/
}