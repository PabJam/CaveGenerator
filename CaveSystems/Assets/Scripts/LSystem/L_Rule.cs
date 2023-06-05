using System.Text;
using UnityEngine;

public static class L_Rule
{
    // The rules of each generation
    public static char[][] rule;

    // The char which gets changed with the rule
    public static char swapchar = 'F';

    public static void GenrateRule(int generations)
    {
        StringBuilder code = new StringBuilder();
        rule = new char[generations][];
        for (int i = 0; i < generations; i++)
        {
            // Length of the rule
            rule[i] = new char[7];
        }

        for (int g = 0; g < rule.Length; g++)
        {
            for (int i = 1; i < rule[g].Length - 1; i++)
            {
                switch (Random.Range(1, 10))
                {
                    case 1:
                        rule[g][i] = 'F';
                        break;

                    case 2:
                        rule[g][i] = '+';
                        break;

                    case 3:
                        rule[g][i] = '-';
                        break;

                    case 4:
                        rule[g][i] = 'S';
                        break;

                    case 5:
                        rule[g][i] = 'L';
                        break;

                    case 6:
                        rule[g][i] = 'U';
                        break;

                    case 7:
                        rule[g][i] = 'D';
                        break;

                    case 8:
                        rule[g][i] = '>';
                        break;

                    case 9:
                        rule[g][i] = '<';
                        break;
                }
            }
            // Adds move forward to beginning and end of each rule
            rule[g][0] = 'F';
            rule[g][rule[g].Length - 1] = 'F';
            for (int i = 0; i < rule[g].Length; i++)
            {
                code.Append(rule[g][i]);
            }
        }
        Debug.Log("Rules: " + code);
    }

    /// <summary>
    /// Reads given rule and sets it to be used
    /// </summary>
    /// <param name="rules">predetermined rule</param>
    public static bool SetRule(string rules)
    {
        int generations = rules.Length / 7;
        if (rules.Length % 7 != 0)
        {
            Debug.LogError("Rules does not have the correct length");
            GenrateRule(generations);
            return false;
        }

        rule = new char[generations][];

        for (int g = 0; g < rule.Length; g++)
        {
            rule[g] = rules.Substring(g * 7, 7).ToCharArray();
        }
        return true;
    }
}