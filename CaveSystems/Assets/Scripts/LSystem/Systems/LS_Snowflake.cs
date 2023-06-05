using System.Text;
using UnityEngine;

public class LS_Snowflake : L_System
{
    /// <summary>
    /// Generates the L_System
    /// </summary>
    public override void GenerateLSystem()
    {
        base.GenerateLSystem();

        // adds all actions that will be used to the alphabet
        alphabet.Add(new LA_Forward());
        alphabet.Add(new LA_RotateCW());
        alphabet.Add(new LA_RotateCCW());
        alphabet.Add(new LA_SavePos());
        alphabet.Add(new LA_LoadPos());
        alphabet.Add(new LA_PitchUp());
        alphabet.Add(new LA_PitchDown());
        alphabet.Add(new LA_StepIncrease());
        alphabet.Add(new LA_StepDecrease());

        // Checks if it should generate new rules or use given
        if (predeterminedRules == true)
        {
            if (L_Rule.SetRule(rules) == true)
            {
                generations = rules.Length / 7;
            }
        }
        else
        {
            L_Rule.GenrateRule(generations);
        }

        // Checks if it should generate a new word to start with or use given
        if (predeterminedAxiom == true)
        {
            word.AddRange(axiom);
        }
        else
        {
            word.Add('F');
            for (int i = 0; i < axiomLength - 1; i++)
            {
                switch (Random.Range(1, 10))
                {
                    case 1:
                        word.Add('F');
                        break;

                    case 2:
                        word.Add('+');
                        break;

                    case 3:
                        word.Add('-');
                        break;

                    case 4:
                        word.Add('S');
                        break;

                    case 5:
                        word.Add('L');
                        break;

                    case 6:
                        word.Add('U');
                        break;

                    case 7:
                        word.Add('D');
                        break;

                    case 8:
                        word.Add('>');
                        break;

                    case 9:
                        word.Add('<');
                        break;
                }
            }
            word.Add('F');

            // Prints the starting word to save it
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < word.Count; i++)
            {
                code.Append(word[i]);
            }
            Debug.Log("Axiom: " + code);
        }

        Iterate();
    }

    /// <summary>
    /// Iterates through the word a set amount of times and applies the rules to it
    /// </summary>
    protected override void Iterate()
    {
        for (int i = 0; i < generations; i++)
        {
            string newWord = new string(word.ToArray());
            string rule = new string(L_Rule.rule[i]);
            newWord = newWord.Replace(L_Rule.swapchar.ToString(), rule);
            word.Clear();
            word.AddRange(newWord);
        }

        ExecuteWord();
    }

    /// <summary>
    /// goes through the generated word and applies every action in order
    /// </summary>
    protected override void ExecuteWord()
    {
        for (int i = 0; i < word.Count; i++)
        {
            for (int j = 0; j < alphabet.Count; j++)
            {
                if (word[i] == alphabet[j].character)
                {
                    alphabet[j].ExecuteAction(this);
                    break;
                }
            }
        }

        L_StructureData.positions = positions;
        SetCornerPoints();
    }

    /// <summary>
    /// calculates the corner points of the cube surrounding it
    /// </summary>
    protected override void SetCornerPoints()
    {
        down = positions[0][0];
        up = positions[0][0];
        left = positions[0][0];
        right = positions[0][0];
        front = positions[0][0];
        back = positions[0][0];
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = 0; j < positions[i].Count; j++)
            {
                if (positions[i][j].y < down.y)
                {
                    down = positions[i][j];
                }
                if (positions[i][j].y > up.y)
                {
                    up = positions[i][j];
                }
                if (positions[i][j].x < left.x)
                {
                    left = positions[i][j];
                }
                if (positions[i][j].x > right.x)
                {
                    right = positions[i][j];
                }
                if (positions[i][j].z < front.z)
                {
                    front = positions[i][j];
                }
                if (positions[i][j].z > back.z)
                {
                    back = positions[i][j];
                }
            }
        }
    }

    /// <summary>
    /// Draws debug lines to see structure
    /// </summary>
    private void Update()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = 1; j < positions[i].Count; j++)
            {
                Debug.DrawLine(positions[i][j - 1], positions[i][j], Color.red);
            }
        }
    }
}