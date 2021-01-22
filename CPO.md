## Supported API and limitations for CPO

### Supported API
Solve and refine conflict methods are executed remotely.
Use name constraints with explicit labels when invoking the conflict refiner as returned conflicts will use these labels to identify the conflicting subset of constraints.

Here is the list of methods that were redefined specifically to run with Watson Machine Learning.

```
public bool RefineConflict()
public void WriteConflict()
public void WriteConflict(TextWriter os)
public double ObjValue { get }
public double[] getObjValues()
public int GetIntValue(IIntVar v)
public double GetValue(INumVar v)
public int GetValue(String intVarName)
public bool IsPresent(String intervalVarName)
public int GetEnd(String intervalVarName)
public void GetValues(IIntVar[] vars, double[] vals)
public void GetValues(INumVar[] varArray, double[] numArray)
public int GetStart(String intervalVarName)
public int GetSize(String intervalVarName)
public int GetLength(String intervalVarName)       
public bool Solve()
```


### Partially supported

The following methods are partially supported: only instances of IIntVarChooser, IIntValueChooser and IIntValueEval are accepted as parameters. Instances derived from abstract ICustomIntVarChooser, ICustomIntValueChooser or ICustomIntValueEval classes are not supported.  

```
public ISearchPhase SearchPhase(IIntVar[] vars, IIntVarChooser varChooser, IIntValueChooser valueChooser)
public ISearchPhase SearchPhase(IIntVarChooser varChooser, IIntValueChooser valueChooser)
public IValueSelector SelectLargest(double minNumber, IIntValueEval e)
public IVarSelector SelectLargest(double minNumber, IIntVarEval e)
public IValueSelector SelectLargest(IIntValueEval e, double tol)
public IValueSelector SelectLargest(IIntValueEval e)
public IVarSelector SelectLargest(IIntVarEval e, double tol)
public IVarSelector SelectLargest(IIntVarEval e)
public IValueSelector SelectSmallest(double minNumber, IIntValueEval e)
public IVarSelector SelectSmallest(double minNumber, IIntVarEval e)
public IValueSelector SelectSmallest(IIntValueEval e, double tol)
public IValueSelector SelectSmallest(IIntValueEval e)
public IVarSelector SelectSmallest(IIntVarEval e, double tol)
public IVarSelector SelectSmallest(IIntVarEval e)
```

### Unsupported API

Any method that is not stated as supported is by definition not supported and may lead to unexpected results/crashes if called.
