using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcProjectManage.Util
{
    public class ArrayChange
    {
        public static int[] StrsToInts(string[] strs)
        {
            int[] result = new int[strs.Length];
            for(int i = 0; i < strs.Length; i++)
            {
                if(strs[i] == "")
                {
                    result[i] = 0;
                }
                else
                {
                    result[i] = int.Parse(strs[i]);
                }
               
            }
            return result;
        }

        public static string StrsToStr(string[] strs,char t = ',')
        {
            string result = "";
            for(int i = 0; i < strs.Length; i++)
            {
                if (i > 0) result += t;
                result += strs[i];
            }
            return result;
        }

        public static string IntsToStr(int[] ints, char t = ',')
        {
            string result = "";
            for (int i = 0; i < ints.Length; i++)
            {
                if (i > 0) result += t;
                result += ints[i];
            }
            return result;
        }

        public static string[] StrsDistinct(string[] strs)
        {
            return strs.Distinct().ToArray();
        }
    }
}