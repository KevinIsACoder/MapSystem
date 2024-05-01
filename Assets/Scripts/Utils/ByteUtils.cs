using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilitarySimulator.Core.Utils
{
    public static class ByteUtils
    {
		
		public static byte[] GetBytes(int n)
		{
			byte[] ib = BitConverter.GetBytes(n);
            if (!BitConverter.IsLittleEndian)
            {
				Array.Reverse(ib);
            }
			return ib;
		}

		public static byte[] GetBytes(float n)
		{
			byte[] ib = BitConverter.GetBytes(n);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(ib);
			}
			return ib;
		}


		public static int ToInt32(byte[] dataptr, int startIndex)
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(dataptr,startIndex,4);
			}

			return BitConverter.ToInt32(dataptr,startIndex);
		}

		public static float ToSingle(byte[] dataptr, int startIndex)
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(dataptr, startIndex, 4);
			}

			return BitConverter.ToSingle(dataptr, startIndex);
		}

	}
}
