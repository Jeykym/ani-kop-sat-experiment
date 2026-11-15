namespace ani_kop_sat_experiment;

public sealed record GsatOptions(int MaxTries = 10, int MaxFlips = 1000, double P = 0.5, int? Seed = null);

public sealed record GsatResult(bool[]? Assignment, int Tries, int TotalFlips)
{
    public bool IsSolved => Assignment is not null;
}

public static class Gsat
{
    public static GsatResult Solve(Formula formula, GsatOptions options)
    {
        var random = options.Seed is { } seed ? new Random(seed) : new Random();
        var assignment = new bool[formula.NumVars + 1];
        var bestCandidates = new List<int>();
        var totalFlips = 0;

        for (var tryNumber = 1; tryNumber <= options.MaxTries; tryNumber++)
        {
            // randomize assignment
            for (var variable = 1; variable < assignment.Length; variable++)
            {
                assignment[variable] = random.Next(2) == 1;
            }

            for (var flipNumber = 0; flipNumber < options.MaxFlips; flipNumber++)
            {
                var unsatisfiedClauses = formula.GetUnsatisfiedClauses(assignment);
                if (unsatisfiedClauses.Count == 0)
                {
                    return new GsatResult(assignment, tryNumber, totalFlips);
                }

                int variableToFlip;

                if (random.NextDouble() < options.P)
                {
                    // random variable from a random unsatisfied clause
                    var chosenClause = unsatisfiedClauses[random.Next(unsatisfiedClauses.Count)];
                    variableToFlip = Math.Abs(chosenClause[random.Next(chosenClause.Count)]);
                }
                else
                {
                    // a random variable among those whose flip leaves the fewest unsatisfied clauses
                    var lowestUnsatisfiedCount = int.MaxValue;
                    bestCandidates.Clear();

                    for (var variable = 1; variable <= formula.NumVars; variable++)
                    {
                        assignment[variable] = !assignment[variable];

                        var unsatisfiedAfterFlip = formula.GetUnsatisfiedClauses(assignment).Count;

                        assignment[variable] = !assignment[variable];

                        if (unsatisfiedAfterFlip < lowestUnsatisfiedCount)
                        {
                            lowestUnsatisfiedCount = unsatisfiedAfterFlip;
                            bestCandidates.Clear();
                        }

                        if (unsatisfiedAfterFlip == lowestUnsatisfiedCount)
                        {
                            bestCandidates.Add(variable);
                        }
                    }

                    variableToFlip = bestCandidates[random.Next(bestCandidates.Count)];
                }

                assignment[variableToFlip] = !assignment[variableToFlip];
                totalFlips++;
            }

            // the last flip of this try may have solved it
            if (formula.GetUnsatisfiedClauses(assignment).Count == 0)
            {
                return new GsatResult(assignment, tryNumber, totalFlips);
            }
        }

        return new GsatResult(null, options.MaxTries, totalFlips);
    }
}
