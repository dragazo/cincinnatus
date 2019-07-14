using System;
using System.Text;
using System.Drawing;

namespace Cincinnatus
{
	public static class Utility
	{
		/// <summary>
		/// Transforms cammel case names into normal text by adding spaces before capitals
		/// </summary>
		/// <param name="str">string to transform</param>
		public static string TransformCammelCase(string str)
		{
			StringBuilder b = new StringBuilder();

			if (str.Length != 0) b.Append(str[0]);
			for (int i = 1; i < str.Length; i++)
			{
				if (char.IsUpper(str[i])) b.Append(' ');
				b.Append(str[i]);
			}

			return b.ToString();
		}
	}
}
