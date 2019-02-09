using System.Collections.Generic;
using UnityEngine;

public class Utils {

    static List<int> randomNums = new List<int>();

    public static int GetRandomInt(int from, int to)
    {
        int iterations = 0;
        while (true)
        {
            iterations++;
            int candidate = Random.Range(from, to);
            if (!randomNums.Contains(candidate))
            {
                randomNums.Add(candidate);
                return candidate;
            }
        }
    }

    public static void ResetRandomIntList()
    {
        randomNums.Clear();
    }

}
