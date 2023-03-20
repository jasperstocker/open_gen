using System.Text;
using UnityEngine;

namespace opengen
{
	public class OpenGenLog
	{
		public static bool AllowLogging = false;

		public static void Log(object input, Object context = null)
		{
			if (AllowLogging)
			{
				Debug.Log(input, context);
			}
		}

		public static void Log(Object context = null, params object[] input)
		{
			if (AllowLogging)
			{
				StringBuilder sb = new();
				foreach (object inputItem in input)
				{
					sb.Append(inputItem);
					sb.Append(" ");
				}
				Debug.Log(sb, context);
			}
		}
		
		public static void ClearDeveloperConsole()
		{
			if (AllowLogging)
			{
				Debug.ClearDeveloperConsole();
			}
		}
	}
}