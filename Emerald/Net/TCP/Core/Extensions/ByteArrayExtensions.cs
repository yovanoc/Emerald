using System;
using System.Collections.Generic;
using System.Text;

namespace Emerald.Net.TCP.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] Add (this byte[] arr1, byte[] arr2)
        {
            var newArray = new byte[arr1.Length + arr2.Length];
            
            Array.Copy(arr1, 0, newArray, 0, arr1.Length);
            Array.Copy(arr2, 0, newArray, arr1.Length, arr2.Length);

            return newArray;
        }
    }
}
