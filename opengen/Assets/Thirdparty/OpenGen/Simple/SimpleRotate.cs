using UnityEngine;

namespace opengen.simple
{
	public class SimpleRotate : MonoBehaviour
	{
		public Vector3 rotation = Vector3.zero;

		private void Update()
		{
			transform.Rotate(rotation * Time.deltaTime);
		}
	}
}