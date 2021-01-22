using ILOG.Concert;
using ILOG.Concert.Cppimpl;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ILOG.Concert.Cppimpl.IloAlgorithm;

namespace ILOG.CP
{
    public abstract class ExternalCP : NotSupportedCP
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ExternalCP));

        private Status status = Status.Error;

        public ExternalCP() : base()
        {
        }

        public override void End()
        {
            base.End();
        }

        /**
         *
         */
        private class VariablesDictionarys
        {
            public Dictionary<String, INumVar> intVars;
            public Dictionary<String, IIntervalVar> intervalVars;
            public Dictionary<String, IIntervalSequenceVar> intervalSequenceVars;
            public Dictionary<String, IStateFunction> stateFunctions;
            public Dictionary<String, String> constraints;

            public VariablesDictionarys(Dictionary<String, INumVar> intVars, Dictionary<String, IIntervalVar> intervalVars,
                                 Dictionary<String, IIntervalSequenceVar> intervalSequenceVars,
                                 Dictionary<String, IStateFunction> stateFunctions,
                                 Dictionary<String, String> constraints)
            {
                this.intVars = intVars;
                this.intervalVars = intervalVars;
                this.intervalSequenceVars = intervalSequenceVars;
                this.stateFunctions = stateFunctions;
                this.constraints = constraints;
            }
        }



        private WmlSolution result = null;

        /**
         * Keep track of original name for variables that are renamed before exporting the model to a ".cpo" file.
         */
        private Dictionary<IAddable, String> renamedVarToOriginName = new Dictionary<IAddable, String>();

        /**
         * Set a unique name in case:
         * - variable has no name (that Is: name == null)
         * - variable name Is a duplicate (same name Is used by multiple variables of same type)
         * After parsing the solution, original names are restored by invoking the restoreRenamedVarsToOriginName() method.
         *
         * @param var           The variable to be assigned a unique name if necessary
         * @param prefix        The prefix to be used for creating new names (when variable name Is null)
         * @param exIstingNames Collection of exIsting names to detect duplicates
         */
        private void SetUniqueName(IAddable addable, String prefix, HashSet<String> exIstingNames)
        {
            String originName = addable.Name;
            String name = (originName == null ? prefix : originName);
            String newName = name;
            int counter = 0;
            while (exIstingNames.Contains(newName))
            {
                counter += 1;
                newName = name + "_" + counter;
            }
            if (!newName.Equals(originName))
            {
                addable.Name = newName;
                renamedVarToOriginName.Add(addable, originName);
            }
        }

        /**
         * Restore original name for model variables that have been renamed before exporting the model to a ".cpo" file.
         */
        private void restoreRenamedVarsToOriginName()
        {
            foreach (var e in renamedVarToOriginName)
            {
                e.Key.Name = e.Value;
            }
        }

        /**
         * If multiple model objects have same name,
         */
        private void checkNoDuplicateNameInModel(String intervalVarName)
        {
            if (renamedVarToOriginName.ContainsValue(intervalVarName))
            {
                throw new Concert.Exception("Duplicate model object name " + intervalVarName);
            }
        }

        /**
         *
         */
        private VariablesDictionarys BuildVariableDictionarys()
        {
            // Clear renamed variables map
            renamedVarToOriginName.Clear();

            Dictionary<String, INumVar> intVars = new Dictionary<String, INumVar>();
            IloIntVarArray allIntVars = GetCPImpl().getAllIloIntVars();
            for (int i = 0; i < allIntVars.getSize(); i++)
            {
                IIntVar v = allIntVars.GetIntVar(i);
                SetUniqueName(v, "IntVar_" + i, new HashSet<string>(intVars.Keys));
                intVars.Add(v.Name, v);
            }

            Dictionary<String, IIntervalVar> intervalVars = new Dictionary<String, IIntervalVar>();
            IloIntervalVarArray allIntervalVars = GetCPImpl().getAllIloIntervalVars();
            for (int i = 0; i < allIntervalVars.getSize(); i++)
            {
                IIntervalVar intervalVar = allIntervalVars.get_IloIntervalVar(i);
                SetUniqueName(intervalVar, "IntervalVar_" + i, new HashSet<string>(intervalVars.Keys));
                intervalVars.Add(intervalVar.Name, intervalVar);
            }

            Dictionary<String, IIntervalSequenceVar> intervalSequenceVars = new Dictionary<String, IIntervalSequenceVar>();

            IloIntervalSequenceVarArray allIloIntervalSequenceVars = GetCPImpl().getAllIloIntervalSequenceVars();
            for (int i = 0; i < allIloIntervalSequenceVars.getSize(); i++)
            {
                IloIntervalSequenceVar intervalSequence = allIloIntervalSequenceVars.get_IloIntervalSequenceVar(i);
                SetUniqueName(intervalSequence, "IntervalSequenceVar_" + i, new HashSet<string>(intervalSequenceVars.Keys));
                intervalSequenceVars.Add(intervalSequence.Name, intervalSequence);
            }

            Dictionary<String, IStateFunction> stateFunctions = new Dictionary<String, IStateFunction>();
            IloStateFunctionArray allIloStateFunctions = GetCPImpl().getAllIloStateFunctions();
            for (int i = 0; i < allIloStateFunctions.getSize(); i++)
            {
                IloStateFunction stateFunction = allIloStateFunctions.get_IloStateFunction(i);
                SetUniqueName(stateFunction, "StateFunction_" + i, new HashSet<string>(stateFunctions.Keys));
                stateFunctions.Add(stateFunction.Name, stateFunction);
            }

            System.Collections.IEnumerator iterator = this.GetEnumerator();
            Dictionary<String, String> constraints = new Dictionary<String, String>();
            int constraint_counter = 0;
            List<IloConstraint> allCts = new List<IloConstraint>();
            while (iterator.MoveNext())
            {
                Object o = iterator.Current;
                if (o is IloConstraint)
                {
                    IloConstraint constraint = (IloConstraint)o;
                    allCts.Add(constraint);
                }
            }
            foreach (var ct in allCts.ToHashSet())
            {
                // Need to retrieve the String representation of the constraint before setting a default name (named constraints are dIsplayed with their name)
                String constraintAsStr = ct.ToString();
                HashSet<String> cts = new HashSet<string>(constraints.Keys);
                constraint_counter = constraint_counter + 1;
                SetUniqueName(ct, "Constraint_" + constraint_counter, cts);
                constraints.Add(ct.Name, constraintAsStr);
            }
            return new VariablesDictionarys(intVars, intervalVars, intervalSequenceVars, stateFunctions, constraints);
        }


        protected void ResetStatus()
        {
            this.status = Status.Error;
        }



        public override bool Solve()
        {
            VariablesDictionarys variableDictionarys = BuildVariableDictionarys();

            // Now perform the solve
            String solveStatus = ExternalSolve();
            logger.Info("SolveStatus = " + solveStatus);

            result = null;
            if (solveStatus.Equals("infeasible_solution"))
            {
                status = Status.Infeasible;
            }
            else
            {
                // We have a feasible solution. Parse the solution file
                result = GetSolution();
                status = result.GetSolutionStatus();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2val)
                {
                    INumVar val;
                    variableDictionarys.intVars.TryGetValue(e.Key, out val);
                    result.var2val.Add(val, e.Value);
                }
                // Keep name2val map as it Is used for Getters using interval name
                //  result.name2val.clear();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2start)
                {
                    IIntervalVar val;
                    variableDictionarys.intervalVars.TryGetValue(e.Key, out val);
                    result.var2start.Add(val, e.Value);
                }
                // Keep name2start map as it Is used for Getters using interval name
                //  result.name2start.clear();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2size)
                {
                    IIntervalVar val;
                    variableDictionarys.intervalVars.TryGetValue(e.Key, out val);
                    result.var2size.Add(val, e.Value);
                }
                // Keep name2size map as it Is used for Getters using interval name
                //  result.name2size.clear();

                // Transfer non-zeros indexed by name to non-zeros indexed by object.
                foreach (var e in result.name2end)
                {
                    IIntervalVar val;
                    variableDictionarys.intervalVars.TryGetValue(e.Key, out val);
                    result.var2end.Add(val, e.Value);
                }
                // Keep name2end map as it Is used for Getters using interval name
                //  result.name2end.clear();

                //
                foreach (var e in result.name2nameList)
                {
                    List<IIntervalVar> intervalList = new List<IIntervalVar>();
                    foreach (String interval_id in e.Value)
                    {
                        IIntervalVar v1;
                        variableDictionarys.intervalVars.TryGetValue(interval_id, out v1);
                        intervalList.Add(v1);
                    }
                    IIntervalSequenceVar v;
                    variableDictionarys.intervalSequenceVars.TryGetValue(e.Key, out v);
                    result.var2intervalList.Add(v, intervalList);
                }

                //
                foreach (var e in result.name2segmentList)
                {
                    IStateFunction v;
                    variableDictionarys.stateFunctions.TryGetValue(e.Key, out v);
                    result.var2segmentList.Add(v, e.Value);
                }
            }

            // Restore original names of renamed variables
            restoreRenamedVarsToOriginName();

            return status.Equals(Status.Feasible) || status.Equals(Status.Optimal);
        }



        public override bool RefineConflict()
        {
            VariablesDictionarys variableDictionarys = BuildVariableDictionarys();

            //
            String solveStatus = ExternalRefineConflict();
            logger.Info("RefineConflict SolveStatus = " + solveStatus);

            result = GetSolution();
            status = result.GetSolutionStatus();

            if (result.hasConflicts)
            {

                // Transfer conflicting constraints indexed by name to conflict indexed by object
                foreach (var conflictingConstraint in result.conflictingConstraints)
                {
                    String val;
                    variableDictionarys.constraints.TryGetValue(conflictingConstraint.Key, out val);
                    result.conflictingConstraintList.Add(val);
                }
                foreach (var conflictingIntervalVar in result.conflictingIntervalVars)
                {
                    IIntervalVar val;
                    variableDictionarys.intervalVars.TryGetValue(conflictingIntervalVar.Key, out val);
                    result.conflictingIntervalVarsList.Add(val.ToString());
                }

                return true;
            }
            return false;
        }



        public override void WriteConflict()
        {
            WriteConflict(Console.Out);
        }



        public override void WriteConflict(TextWriter os)
        {
            if (result == null)
                throw new Concert.Exception("No solution available");
            if (result.hasConflicts)
            {
                os.WriteLine("// ------ Conflict members: ---------------------------------------------------");
                foreach (String constraint in result.conflictingConstraintList)
                {
                    os.WriteLine(constraint);

                }
                foreach (String intervalVar in result.conflictingIntervalVarsList)
                {
                    os.WriteLine(intervalVar);
                }
            }
            os.Close();
        }

        /**
         * Perform an external solve.
         */
        protected abstract String ExternalSolve();

        protected abstract String ExternalRefineConflict();

        public virtual WmlSolution GetSolution()
        {
            throw new Concert.Exception("Implement me");
        }

        // Below we overwrite a bunch of CP functions that query solutions.
        // Add your own overwrites if you need more.


        public override double ObjValue {
            get {
                if (result == null)
                    throw new Concert.Exception("No solution available");
                return getObjValues()[0]; // Returns first objective value in lIst
            }
        }
        public double[] getObjValues()
        {
            if (result == null)
                throw new Concert.Exception("No solution available");
            if (result.objectiveValues == null)
                throw new Concert.Exception("No objective defined");
            double[] res = new double[result.objectiveValues.Count];
            for (int index = 0; index < res.Length; index++)
            {
                res[index] = result.objectiveValues[index];
            }
            return res;
        }

        public override int GetIntValue(IIntVar v)
        {
            if (result == null)
                return base.GetIntValue(v);
            if (result.var2val.ContainsKey(v))
            {
                double val;
                result.var2val.TryGetValue(v, out val);
                return Convert.ToInt32(val);
            }
            else
                return base.GetIntValue(v);
        }


        public override double GetValue(INumVar v)
        {
            if (result == null)
                return base.GetValue(v);
            if (result.var2val.ContainsKey(v))
            {
                double val;
                result.var2val.TryGetValue(v, out val);
                return val;
            }
            else
                return base.GetValue(v);
        }


        public override int GetValue(String intVarName)
        {
            checkNoDuplicateNameInModel(intVarName);
            if (result == null)
                return base.GetValue(intVarName);
            if (result.name2val.ContainsKey(intVarName))
            {
                double val;
                result.name2val.TryGetValue(intVarName, out val);
                return Convert.ToInt32(val);
            }
            else
                return base.GetValue(intVarName);
        }


        public override bool IsPresent(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            if (result == null)
                return false;
            return result.name2start.ContainsKey(intervalVarName);
        }

        public override bool IsAbsent(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            return !IsPresent(intervalVarName);
        }

        public override int GetEnd(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            if (result == null)
                return base.GetEnd(intervalVarName);
            if (result.name2end.ContainsKey(intervalVarName))
            {
                double val;
                result.name2end.TryGetValue(intervalVarName, out val);
                return Convert.ToInt32(val);
            }
            else
                return base.GetEnd(intervalVarName);
        }


        public override void GetValues(IIntVar[] vars, double[] vals)
        {
            if (result == null)
                base.GetValues(vars, vals);
            if ((vars == null) || (vals == null))
                throw new Concert.Exception("InPut arrays should not be null");
            if (vars.Length != vals.Length)
                throw new Concert.Exception("InPut arrays Length mIsmatch");
            for (int i = 0; i < vars.Length; i++)
                vals[i] = GetValue(vars[i]);
        }


        public override void GetValues(INumVar[] varArray, double[] numArray)
        {
            if (result == null)
                base.GetValues(varArray, numArray);
            if ((varArray == null) || (numArray == null))
                throw new Concert.Exception("InPut arrays should not be null");
            if (varArray.Length != numArray.Length)
                throw new Concert.Exception("InPut arrays Length mIsmatch");
            for (int i = 0; i < varArray.Length; i++)
                numArray[i] = GetValue(varArray[i]);
        }


        public override int GetStart(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            if (result == null)
                return base.GetStart(intervalVarName);
            if (result.name2end.ContainsKey(intervalVarName))
            {
                double val;
                result.name2start.TryGetValue(intervalVarName, out val);
                return Convert.ToInt32(val);
            }
            else
                return base.GetStart(intervalVarName);
        }


        public override int GetSize(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            if (result == null)
                return base.GetSize(intervalVarName);
            if (result.name2size.ContainsKey(intervalVarName))
            {
                double val;
                result.name2size.TryGetValue(intervalVarName, out val);
                return Convert.ToInt32(val);
            }
            else
                return base.GetSize(intervalVarName);
        }


        public override int GetLength(String intervalVarName)
        {
            checkNoDuplicateNameInModel(intervalVarName);
            if (result == null)
                return base.GetSize(intervalVarName);
            return GetEnd(intervalVarName) - GetStart(intervalVarName);
        }


        public override ISearchPhase SearchPhase(IIntVar[] vars, IIntVarChooser varChooser, IIntValueChooser valueChooser)
        {
            if (!(varChooser is IIntVarChooser))
            {
                logger.Error(notSupportedError + " searchPhase with custom IIntVarChooser");
                throw new Concert.Exception(notSupportedError);
            }
            if (!(valueChooser is IIntValueChooser))
            {
                logger.Error(notSupportedError + " searchPhase with custom IloIntValueChooser");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SearchPhase(vars, varChooser, valueChooser);
        }


        public override ISearchPhase SearchPhase(IIntVarChooser varChooser, IIntValueChooser valueChooser)

        {
            if (!(varChooser is IIntVarChooser))
            {
                logger.Error(notSupportedError + " searchPhase with custom IIntVarChooser");
                throw new Concert.Exception(notSupportedError);
            }
            if (!(valueChooser is IIntValueChooser))
            {
                logger.Error(notSupportedError + " searchPhase with custom IloIntValueChooser");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SearchPhase(varChooser, valueChooser);
        }


        public override IValueSelector SelectLargest(double minNumber, IIntValueEval e)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(minNumber, e);
        }


        public override IVarSelector SelectLargest(double minNumber, IIntVarEval e)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(minNumber, e);
        }


        public override IValueSelector SelectLargest(IIntValueEval e, double tol)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(e, tol);
        }


        public override IValueSelector SelectLargest(IIntValueEval e)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(e);
        }


        public override IVarSelector SelectLargest(IIntVarEval e, double tol)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(e, tol);
        }


        public override IVarSelector SelectLargest(IIntVarEval e)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectLargest(e);
        }


        public override IValueSelector SelectSmallest(double minNumber, IIntValueEval e)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(minNumber, e);
        }


        public override IVarSelector SelectSmallest(double minNumber, IIntVarEval e)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(minNumber, e);
        }


        public override IValueSelector SelectSmallest(IIntValueEval e, double tol)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(e, tol);
        }

        public override IValueSelector SelectSmallest(IIntValueEval e)
        {
            if (!(e is IIntValueEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IloIntValueEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(e);
        }


        public override IVarSelector SelectSmallest(IIntVarEval e, double tol)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(e, tol);
        }

        public override IVarSelector SelectSmallest(IIntVarEval e)
        {
            if (!(e is IIntVarEval))
            {
                logger.Error(notSupportedError + " SelectLargest with custom IIntVarEval");
                throw new Concert.Exception(notSupportedError);
            }
            return base.SelectSmallest(e);
        }

    }

}
