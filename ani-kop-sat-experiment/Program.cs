using ani_kop_sat_experiment;

var formula = SampleFormulas.Uf20_01;

foreach (var p in new[] { 0.0, 0.5 })
{
    for (var seed = 0; seed < 5; seed++)
    {
        var result = Gsat.Solve(formula, new GsatOptions(P: p, Seed: seed));
        var verified = result.IsSolved && Gsat.GetUnsatisfiedClauses(formula, result.Assignment!).Count == 0;

        Console.WriteLine(result.IsSolved
            ? $"p={p} seed={seed}: SAT, try {result.Tries}, {result.TotalFlips} flips, verified: {verified}"
            : $"p={p} seed={seed}: not solved after {result.Tries} tries, {result.TotalFlips} flips");
    }
}
