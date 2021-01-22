## Supported API and limitations for CPLEX

##### Supported API

Here is the list of methods that are currently supported:
```
public double GetObjValue()
public double GetValue(INumVar v)
public double[] GetValues(INumVar[] v)
public double[] GetValues(INumVar[] var1, int var2, int var3)
public double GetDual(IRange r)
public bool Solve()
public double[] GetDuals(IRange[] r)
public Status GetStatus()
public CplexStatus GetCplexStatus()
public double GetReducedCost(INumVar v)
public double[] GetReducedCosts(INumVar[] var1, int var2, int var3)
public double[] GetDuals(IRange[] var1, int var2, int var3)
public double GetSlack(IRange r)
public double[] GetSlacks(IRange[] r)
public double[] GetSlacks(IRange[] var1, int var2, int var3)
public double GetValue(INumExpr var)
public double GetValue(IIntExpr var)
public bool FeasOpt(IConstraint[] var1, double[] var2)
public double[] GetValues(ILPMatrix matrix);
public double[] GetSlacks(ILPMatrix matrix);
public double[] GetReducedCosts(ILPMatrix matrix);
public double[] GetDuals(ILPMatrix matrix);
```


##### Unsupported API

Callbacks/goals, solution pools, asynchronous apis, conflicts and some relaxation methods are not supported.
Any method that is not stated as supported is by definition not supported and may lead to unexpected results/crashes if called.
