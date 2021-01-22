using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using ILOG.Concert;
using System.Collections;

namespace ILOG.CPLEX
{
    public class ExternalCplex : NotSupportedCplex
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ExternalCplex));

        private LinkedList<INumVar> _variables = new LinkedList<INumVar>();

        public override void End()
        {
            _variables.Clear();
            base.End();
        }

        public ExternalCplex() : base()
        {
        }

        private void AddVariable(Dictionary<String, INumVar> name2var, Dictionary<INumVar, String> oldNames, INumVar v)
        {
            String name = v.Name;

            if (!oldNames.ContainsKey(v))
            {
                oldNames.Add(v, name); // even if name == null!
                name = String.Format("vv{0}", oldNames.Count);
                v.Name = name;
                name2var.Add(name, v);
            }
        }

        private void AddRange(Dictionary<String, IRange> name2rng, Dictionary<IRange, String> oldNames, IRange v)
        {
            String name = v.Name;
            if (!oldNames.ContainsKey(v))
            {
                oldNames.Add(v, name); // even if name == null!
                name = String.Format("cc{0}", oldNames.Count);
                v.Name = name;
                name2rng.Add(name, v);
            }
        }

        protected Solution result = null;


        public override ISemiContVar SemiContVar(double var1, double var3, NumVarType var5, String var6)
        {
            ISemiContVar x = base.SemiContVar(var1, var3, var5, var6);
            _variables.AddLast(x);
            return x;
        }

        public override ISemiContVar SemiContVar(double var1, double var3, NumVarType var5)
        {
            ISemiContVar x = base.SemiContVar(var1, var3, var5);
            _variables.AddLast(x);
            return x;
        }

        public override ISemiContVar SemiContVar(Column var1, double var2, double var4, NumVarType var6, String var7)
        {
            ISemiContVar x = base.SemiContVar(var1, var2, var4, var6, var7);
            _variables.AddLast(x);
            return x;
        }


        public override INumVar NumVar(double var1, double var3, NumVarType var5)
        {
            INumVar x = base.NumVar(var1, var3, var5);
            _variables.AddLast(x);
            return x;
        }

        public override INumVar NumVar(double var1, double var3, NumVarType var5, String var6)
        {
            INumVar x = base.NumVar(var1, var3, var5, var6);
            _variables.AddLast(x);
            return x;
        }

        public override INumVar NumVar(Column var1, double var2, double var4, NumVarType var6)
        {
            INumVar x = base.NumVar(var1, var2, var4, var6);
            _variables.AddLast(x);
            return x;
        }

        public override INumVar NumVar(Column var1, double var2, double var4, NumVarType var6, String var7)
        {
            INumVar x = base.NumVar(var1, var2, var4, var6, var7);
            _variables.AddLast(x);
            return x;
        }

        private bool _feasOpt(IConstraint[] cts, double[] prefs, IRange[] ranges, double[] rlbs, double[] rubs, INumVar[] vars, double[] vlbs, double[] vubs)
        {
            String badCall = "Bad call to internal feasopt";
            Relaxations relaxer = new Relaxations();
            if (cts != null)
            {
                if (prefs == null) throw new ILOG.Concert.Exception(badCall);
                if (prefs.Length != cts.Length) throw new ILOG.Concert.Exception(badCall);
                for (int i = 0; i < cts.Length; i++)
                {
                    if (cts[i] is IRange)
                        relaxer.Add(cts[i], prefs[i]);
                    else
                        throw new ILOG.Concert.Exception("Only ranges are supported by the feasopt.");
                }
            }
            else
            {
                if (prefs != null) throw new ILOG.Concert.Exception(badCall);
                if (ranges != null)
                {
                    if (rlbs == null) throw new ILOG.Concert.Exception(badCall);
                    if (ranges.Length != rlbs.Length) throw new ILOG.Concert.Exception(badCall);
                    if (rubs == null) throw new ILOG.Concert.Exception(badCall);
                    if (ranges.Length != rubs.Length) throw new ILOG.Concert.Exception(badCall);
                    for (int i = 0; i < ranges.Length; i++)
                        relaxer.Add(ranges[i], rlbs[i], rubs[i]);
                }
                if (vars != null)
                {
                    if (vlbs == null) throw new ILOG.Concert.Exception(badCall);
                    if (vars.Length != vlbs.Length) throw new ILOG.Concert.Exception(badCall);
                    if (vubs == null) throw new ILOG.Concert.Exception(badCall);
                    if (vars.Length != vubs.Length) throw new ILOG.Concert.Exception(badCall);
                    for (int i = 0; i < vars.Length; i++)
                        relaxer.Add(vars[i], vlbs[i], vubs[i]);
                }
            }

            return Process(relaxer, null);
        }

        public override bool Solve()
        {
            return Process(null, null);
        }
        private bool Process(Relaxations relaxer, Conflicts conflicts)
        {
            Dictionary<INumVar, String> oldVarNames = new Dictionary<INumVar, String>(_variables.Count);

            Dictionary<IRange, String> oldRngNames = new Dictionary<IRange, String>(this.Nrows);

            try
            {
                // In order to consume a solution file, _all_ variables must have
                // a name! Go through the model and collect all variables, thereby
                // checking that they have a name and names are unique.
                Dictionary<String, INumVar> vars = new Dictionary<String, INumVar>(_variables.Count);
                Dictionary<String, IRange> rngs = new Dictionary<String, IRange>();
                long t1 = DateTime.Now.Ticks;
                foreach (INumVar v in _variables)
                    AddVariable(vars, oldVarNames, v);
                

                for (IEnumerator it = GetEnumerator(); it.MoveNext(); /* nothing */)
                {
                    Object o = it.Current;
                    if (o is ILPMatrix)
                    {
                        // ignore
                        ILPMatrix matrix = (ILPMatrix)o;
                        IRange[] ranges = matrix.Ranges;
                        foreach (IRange r in ranges)
                            AddRange(rngs, oldRngNames, r);
                    }
                    else if (o is IObjective)
                    {
                        // ignore
                    }
                    else if (o is IRange)
                    {
                        AddRange(rngs, oldRngNames, (IRange)o);
                    }
                    else if (o is INumVar)
                    {
                        // ignore
                    }
                    else if (o is IConversion)
                    {
                        // ignore
                    }
                    else
                        throw new ILOG.Concert.Exception("Cannot handle " + o);
                }

                long t2 = DateTime.Now.Ticks;
                logger.Info("Naming stategy took " + (t2 - t1) / 10000 + " milli seconds.");
                // Now perform the solve
                result = ExternalSolve(vars.Keys.ToHashSet(), rngs.Keys.ToHashSet(), relaxer, conflicts);

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2val)
                {
                    INumVar v;
                    vars.TryGetValue(e.Key, out v);
                    result.var2val.Add(v, e.Value);
                }
                result.name2val.Clear();
                foreach (var e in result.name2ReducedCost)
                {
                    INumVar v;
                    vars.TryGetValue(e.Key, out v);
                    result.var2ReducedCost.Add(v, e.Value);
                }
                result.name2ReducedCost.Clear();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2dual)
                {
                    IRange r;
                    rngs.TryGetValue(e.Key, out r);
                    result.rng2dual.Add(r, e.Value);
                }
                foreach (var r in oldRngNames) {
                 if (result.rng2dual.ContainsKey(r.Key) == false)
                    result.rng2dual.Add(r.Key, 0.0);
                }
                result.name2dual.Clear();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2slack)
                {
                    IRange r;
                    rngs.TryGetValue(e.Key, out r);
                    result.rng2slack.Add(r, e.Value);
                }
                foreach (var r in oldRngNames) {
                    if (result.rng2slack.ContainsKey(r.Key) == false)
                        result.rng2slack.Add(r.Key, 0.0);
                }
                result.name2slack.Clear();

                return result.feasible;
            }
            finally
            {
                // Restore original names if necessary.
                foreach (var e in oldVarNames)
                    e.Key.Name = e.Value;

                // Restore original names if necessary.
                foreach (var e in oldRngNames)
                    e.Key.Name = e.Value;
            }
        }

        /**
         * Perform an external solve.
         * The function must not return <code>null</code>.
         * All fields but {@link Solution#var2val} must be setup in the returned {@link Solution} instance.
         * Field {@link Solution#var2val} will be setup in {@link #solve()} from {@link Solution#name2val}. The latter
         * will also be cleared in {@link #solve()}.
         *
         * @param variables The names of variables known to the solver.
         * @return Solution information for the solve.
         * @ if anything goes wrong.
         */
        public virtual Solution ExternalSolve(HashSet<String> variables, HashSet<String> ranges, Relaxations relax, Conflicts conflicts)
        {
            throw new Concert.Exception("Need to implement Externalsolve");
        }

        // Below we overwrite a bunch of ICplex functions that query solutions.
        // Add your own overwrites if you need more.

        public override double GetObjValue()
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            return result.objective;
        }



        public override double GetValue(INumVar v)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            Double d;
            if (result.var2val.TryGetValue(v, out d) == false)
                throw new ILOG.Concert.Exception("Impossible to query variable value: Unkown variable " + v + " in the solution.");
            return d;
        }


        public override double[] GetValues(INumVar[] v)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; ++i)
            {
                ret[i] = GetValue(v[i]);
            }
            return ret;
        }


        public override double[] GetValues(INumVar[] var1, int var2, int var3)
        {
            int size = var3 - var2;
            if (size < 0) throw new ILOG.Concert.Exception("Cannot Get reduced cost: " + var2 + " " + var3);
            if (size == 0) return new double[0];
            double[] ret = new double[size];
            for (int i = var2; i < var3; i++)
                ret[i - var2] = GetValue(var1[i]);
            return ret;
        }

        public override double GetDual(IRange r)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            Double d;
            if (result.rng2dual.TryGetValue(r, out d) == false)
                throw new ILOG.Concert.Exception("Impossible to query dual for range: Unknown range " + r + " in the solution");
            return d;
        }


        public override double[] GetDuals(IRange[] r)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            double[] ret = new double[r.Length];
            for (int i = 0; i < r.Length; ++i)
            {
                ret[i] = GetDual(r[i]);
            }
            return ret;
        }


        public override Status GetStatus()
        {
            if (result == null)
                return Status.Unknown;
            return CplexHelper.MakeStatus(GetCplexStatus().GetHashCode(), result.pfeas, result.dfeas);
        }


        public override CplexStatus GetCplexStatus()
        {
            if (result == null)
                return CplexStatus.Unknown;
            return CplexHelper.GetStatus(result.status);
        }


        public override double GetReducedCost(INumVar v)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            Double d;

            if (result.var2ReducedCost.TryGetValue(v, out d) == false)
                throw new ILOG.Concert.Exception("Impossible to Get the reduced cost: Unkown variable " + v + " in the solution");
            return d;
        }

        public override double[] GetReducedCosts(INumVar[] var1, int var2, int var3)
        {
            int size = var3 - var2;
            if (size < 0) throw new ILOG.Concert.Exception("Cannot Get reduced cost: " + var2 + " " + var3);
            if (size == 0) return new double[0];
            double[] ret = new double[size];
            for (int i = var2; i < var3; i++)
                ret[i - var2] = GetReducedCost(var1[i]);
            return ret;
        }


        public override double[] GetDuals(IRange[] var1, int var2, int var3)
        {
            int size = var3 - var2;
            if (size < 0) throw new ILOG.Concert.Exception("Cannot Get Duals: " + var2 + " " + var3);
            if (size == 0) return new double[0];
            IRange[] values = new IRange[size];
            for (int i = var2; i < var3; i++)
                values[i - var2] = var1[i];
            return GetDuals(values);
        }

        public override double GetSlack(IRange r)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            Double d;
            if (result.rng2slack.TryGetValue(r, out d) == false)
                throw new ILOG.Concert.Exception("Impossible to query the slack for range " + r + ": Unkown range in the solution");
            return d;
        }


        public override double[] GetSlacks(IRange[] r)
        {
            if (result == null)
                throw new ILOG.Concert.Exception("No solution available");
            double[] ret = new double[r.Length];
            for (int i = 0; i < r.Length; ++i)
            {
                ret[i] = GetSlack(r[i]);
            }
            return ret;
        }

        public override double[] GetSlacks(IRange[] var1, int var2, int var3)
        {
            int size = var3 - var2;
            if (size < 0) throw new ILOG.Concert.Exception("Cannot Get Duals: " + var2 + " " + var3);
            if (size == 0) return new double[0];
            IRange[] values = new IRange[size];
            for (int i = var2; i < var3; i++)
                values[i - var2] = var1[i];
            return GetSlacks(values);
        }


        public override bool IsPrimalFeasible()
        {
            // "real cplex does not always return the real value: sometimes false even if true. So instead of raising an Error, raise a warning.
            logger.Info("isPrimalFeasible not implemented: always return false");
            return false;
        }

        public override bool IsDualFeasible()
        {
            // "real cplex does not always return the real value: sometimes false even if true. So instead of raising an Error, raise a warning.
            logger.Info("isDualFeasible not implemented: always return false");
            return false;
        }

        private double computeQuadExprSum(ILQNumExpr quad)
        {
            IQuadNumExprEnumerator it = quad.GetQuadEnumerator();
            double res = 0.0;
            while (it.MoveNext())
            {
                res += GetValue(it.GetNumVar1()) * GetValue(it.GetNumVar2()) * it.GetValue();
            }
            res += computeLinearExprSum(quad);
            return res;
        }
        private double computeQuadExprSum(ILQIntExpr quad)
        {
            IQuadIntExprEnumerator it = quad.GetQuadEnumerator();
            double res = 0.0;
            while (it.MoveNext())
            {
                res += GetValue(it.GetIntVar1()) * GetValue(it.GetIntVar2()) * it.GetValue();
            }
            res += computeLinearExprSum(quad);
            return res;
        }
        private double computeLinearExprSum(ILinearNumExpr linear)
        {
            ILinearNumExprEnumerator it = linear.GetLinearEnumerator();
            double res = 0.0;
            while (it.MoveNext())
            {
                double val = GetValue(it.NumVar) * it.Value;
                res += val;
            }
            return res;
        }
        private double computeLinearExprSum(ILinearIntExpr linear)
        {
            ILinearIntExprEnumerator it = linear.GetLinearEnumerator();
            double res = 0.0;
            while (it.MoveNext())
            {
                double val = it.Value * GetValue(it.IntVar);
                res += val;
            }
            return res;
        }

        public override double GetValue(INumExpr var)
        {
            if (var is ILQNumExpr)
            {
                ILQNumExpr q = (ILQNumExpr)var;
                return computeQuadExprSum(q) + q.Constant;
            }
            if (var is ILQIntExpr)
            {
                ILQIntExpr q = (ILQIntExpr)var;
                return computeQuadExprSum(q) + q.Constant;
            }
            logger.Error(notSupportedError + " GetValue: non quad expr, non linear expr");
            throw new Concert.Exception(notSupportedError);
        }



        public override bool FeasOpt(IConstraint[] var1, double[] var2)
        {
            return _feasOpt(var1, var2, null, null, null, null, null, null);
        }


        public override bool FeasOpt(IRange[] var1, double[] var2, double[] var3, INumVar[] var4, double[] var5, double[] var6)
        {
            logger.Error(notSupportedError + " feasopt");
            throw new ILOG.Concert.Exception(notSupportedError);
            //return _feasOpt(null, null, var1, var2, var3, var4,var5,var6);
        }


        public override bool FeasOpt(INumVar[] var1, double[] var2, double[] var3)
        {
            logger.Error(notSupportedError + " feasopt");
            throw new ILOG.Concert.Exception(notSupportedError);
            //return _feasOpt(null, null, null, null, null, var1,var2,var3);
        }


        public override bool FeasOpt(IRange[] var1, double[] var2, double[] var3)
        {
            logger.Error(notSupportedError + " feasopt");
            throw new ILOG.Concert.Exception(notSupportedError);
            //return _feasOpt(null, null, var1, var2, var3, null,null,null);
        }


        public override ConflictStatus[] GetConflict(IConstraint[] var1)
        {
            logger.Error(notSupportedError + " GetConflict");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override ConflictStatus GetConflict(IConstraint var1)
        {
            IConstraint[]
            var2 = new IConstraint[] { var1 };
            return GetConflict(var2)[0];
        }

        public override ConflictStatus[] GetConflict(IConstraint[] var1, int var2, int var3)
        {
            int size = var3 - var2;
            if (size < 0) throw new ILOG.Concert.Exception("Problem with conflict");
            if (size == 0) throw new ILOG.Concert.Exception("Problem with conflict");
            IConstraint[] cts = new IConstraint[size];
            for (int i = var2; i < var3; i++)
            {
                cts[i - var2] = var1[i];
            }
            return GetConflict(cts);
        }


        public override bool RefineConflict(IConstraint[] var1, double[] var2)
        {
            logger.Error(notSupportedError + " refineConflict");
            throw new ILOG.Concert.Exception(notSupportedError);
            /*if (var2.Length != var1.Length) throw new ILOG.Concert.Exception("Problem with conflict");
            Conflicts refiner = new Conflicts();
            for (int i = 0; i< var1.Length; i++)
                refiner.add(var1[i], var2[i]);

            return process(null, refiner);*/
        }


        public override bool RefineConflict(IConstraint[] var1, double[] var2, int var3, int var4)
        {
            if (var2.Length != var1.Length) throw new ILOG.Concert.Exception("Problem with conflict");
            int size = var4 - var3;
            if (size < 0) throw new ILOG.Concert.Exception("Problem with conflict");
            if (size == 0) throw new ILOG.Concert.Exception("Problem with conflict");
            IConstraint[] cts = new IConstraint[size];
            double[] prefs = new double[size];
            for (int i = var3; i < var4; i++)
            {
                cts[i - var3] = var1[i];
                prefs[i - var3] = var2[i];
            }
            return RefineConflict(cts, prefs);
        }
    }
}