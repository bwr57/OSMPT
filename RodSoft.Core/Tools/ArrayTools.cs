using System;
using System.Collections.Generic;

namespace RodSoft.Core.Tools
{
    public static class ArrayTools
    {
        public static TSource[] SubArray<TSource>(this TSource[] source, int initialIndex, int count)
        {
            TSource[] subArray = new TSource[count];
            if (source != null && source.Length >= initialIndex + count)
            {
                for (int i = 0; i < count; i++)
                {
                    subArray[i] = source[initialIndex + i];
                }
            }
            return subArray;
        }

        public static void CopyTo<TSource>(this TSource[] source, TSource[] destination, int sourceStartIndex, int destinationStartIndex, int count)
        {
            if(source != null && destination != null)
            {
                int index = 0;
                while(sourceStartIndex + index < source.Length && destinationStartIndex + index < destination.Length && index < count)
                {
                    destination[destinationStartIndex + index] = source[sourceStartIndex + index];
                    index++;
                }
            }
        }

        public static void AddOrReplace<TTarget, TKey, TItem>(this TTarget dictionary, TKey key, TItem item) where TTarget : IDictionary<TKey, TItem>
        {
            if(dictionary != null)
            {
                if(dictionary.ContainsKey(key))
                {
                    dictionary[key] = item;
                }
                else
                {
                    dictionary.Add(key, item);
                }
            }
        }

        public static string ReplaceVb<T>(this T originalString) 
        {
            if (!(originalString is string))
                return null;
            return (originalString as String).Replace("\n", "\r\n");
        }
    }
}
