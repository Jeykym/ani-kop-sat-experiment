namespace ani_kop_sat_experiment;

/// <param name="Cm">Exponent of make in f(x) = make^Cm / (Eps + break)^Cb.</param>
/// <param name="Cb">Exponent of break in f(x) = make^Cm / (Eps + break)^Cb.</param>
/// <param name="Eps">Small value that prevents division by zero when break = 0.</param>
public sealed record ProbSatOptions(
    int MaxTries = 10,
    int MaxFlips = 1000,
    double Cm = 0.0,
    double Cb = 2.3,
    double Eps = 1.0,
    int? Seed = null);

public sealed record ProbSatResult(bool[]? Assignment, int Tries, int TotalFlips)
{
    public bool IsSolved => Assignment is not null;
}

public static class ProbSat
{
    public static ProbSatResult Solve(Formula formula, ProbSatOptions options)
    {
        var random = options.Seed is { } seed ? new Random(seed) : new Random();
        var assignment = new bool[formula.NumVars + 1];
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
                    return new ProbSatResult(assignment, tryNumber, totalFlips);
                }

                // a random unsatisfied clause
                var chosenClause = unsatisfiedClauses[random.Next(unsatisfiedClauses.Count)];

                // weight each of its variables by f(x)
                var weights = new double[chosenClause.Count];
                for (var i = 0; i < chosenClause.Count; i++)
                {
                    weights[i] = Weight(Math.Abs(chosenClause[i]), formula, assignment, options);
                }

                // pick a variable with probability proportional to its weight
                var threshold = random.NextDouble() * weights.Sum();
                var chosenIndex = 0;
                while (chosenIndex < weights.Length - 1 && threshold >= weights[chosenIndex])
                {
                    threshold -= weights[chosenIndex];
                    chosenIndex++;
                }

                var variableToFlip = Math.Abs(chosenClause[chosenIndex]);
                assignment[variableToFlip] = !assignment[variableToFlip];
                totalFlips++;
            }

            // the last flip of this try may have solved it
            if (formula.GetUnsatisfiedClauses(assignment).Count == 0)
            {
                return new ProbSatResult(assignment, tryNumber, totalFlips);
            }
        }

        return new ProbSatResult(null, options.MaxTries, totalFlips);
    }

    // f(x, F) = make^cm / (eps + break)^cb
    private static double Weight(int variable, Formula formula, bool[] assignment, ProbSatOptions options)
    {
        // number of clauses which become satisfied after the flip of x
        var makes = formula.CountUnsatisfiedClauses(assignment, variable);
        
        assignment[variable] = !assignment[variable];
        
        // number of clauses which cease to be satisfied after the flip of x
        var breaks = formula.CountUnsatisfiedClauses(assignment, variable);
        
        assignment[variable] = !assignment[variable];

        return Math.Pow(makes, options.Cm) / Math.Pow(options.Eps + breaks, options.Cb);
    }
}
