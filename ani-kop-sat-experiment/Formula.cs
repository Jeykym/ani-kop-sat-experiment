namespace ani_kop_sat_experiment;

public sealed class Formula
{
    public int NumVars { get; }
    public int NumClauses { get; }
    public IReadOnlyList<IReadOnlyList<int>> Clauses { get; }

    public Formula(int numVars, int numClauses, IEnumerable<IEnumerable<int>> clauses)
    {
        if (numVars < 1)
            throw new ArgumentOutOfRangeException(nameof(numVars), "Formula needs at least one variable.");

        var copy = clauses.Select(c => Array.AsReadOnly(c.ToArray())).ToArray();
        if (copy.Length != numClauses)
            throw new ArgumentException($"Expected {numClauses} clauses, got {copy.Length}.", nameof(clauses));

        for (var c = 0; c < copy.Length; c++)
        {
            if (copy[c].Count == 0)
                throw new ArgumentException($"Clause {c} is empty.", nameof(clauses));
            
            foreach (var lit in copy[c])
                if (lit == 0 || Math.Abs(lit) > numVars)
                    throw new ArgumentException($"Clause {c}: literal {lit} out of range ±1..{numVars}.", nameof(clauses));
        }

        NumVars = numVars;
        NumClauses = numClauses;
        Clauses = Array.AsReadOnly(copy);
    }

    public List<IReadOnlyList<int>> GetUnsatisfiedClauses(bool[] assignment)
    {
        var unsatisfiedClauses = new List<IReadOnlyList<int>>();
        foreach (var clause in Clauses)
        {
            var isClauseSatisfied = false;
            foreach (var literal in clause)
            {
                var isLiteralTrue = literal > 0 ? assignment[literal] : !assignment[-literal];
                if (isLiteralTrue)
                {
                    isClauseSatisfied = true;
                    break;
                }
            }

            if (!isClauseSatisfied)
                unsatisfiedClauses.Add(clause);
        }
        return unsatisfiedClauses;
    }

    // number of unsatisfied clauses that contain the variable
    public int CountUnsatisfiedClauses(bool[] assignment, int variable) =>
        GetUnsatisfiedClauses(assignment).Count(clause => clause.Any(literal => Math.Abs(literal) == variable));
}
