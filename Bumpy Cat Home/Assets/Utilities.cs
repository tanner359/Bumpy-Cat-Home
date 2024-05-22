using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BumpyCat{
    public static class Utils
    {
        public static T[] Array_Add_Item<T>(this T[] array, T data){
            T[] temp = new T[array.Length+1];
            array.CopyTo(temp, 0);
            temp[temp.Length-1] = data;
            return temp;
        }
    }
}