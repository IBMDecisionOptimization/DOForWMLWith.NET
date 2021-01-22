using System;
using System.Collections.Generic;
using System.Linq;
using ILOG.Concert;
using static ILOG.Concert.Cppimpl.IloAlgorithm;
using Newtonsoft.Json.Linq;

namespace ILOG.CP
{
    public class WmlSolution
    {

        /**
         * Dictionary variable names to values.
         */
        public Dictionary<String, Double> name2val = new Dictionary<String, Double>();
        public Dictionary<String, Double> name2start = new Dictionary<String, Double>();
        public Dictionary<String, Double> name2size = new Dictionary<String, Double>();
        public Dictionary<String, Double> name2end = new Dictionary<String, Double>();
        public Dictionary<String, List<String>> name2nameList = new Dictionary<String, List<String>>();
        public Dictionary<String, List<Segment>> name2segmentList = new Dictionary<String, List<Segment>>();
        /**
         * Dictionary variable objects to values.
         */
        public Dictionary<INumVar, Double> var2val = new Dictionary<INumVar, Double>();
        public Dictionary<IIntervalVar, Double> var2start = new Dictionary<IIntervalVar, Double>();
        public Dictionary<IIntervalVar, Double> var2size = new Dictionary<IIntervalVar, Double>();
        public Dictionary<IIntervalVar, Double> var2end = new Dictionary<IIntervalVar, Double>();
        public Dictionary<IIntervalSequenceVar, List<IIntervalVar>> var2intervalList = new Dictionary<IIntervalSequenceVar, List<IIntervalVar>>();
        public Dictionary<IStateFunction, List<Segment>> var2segmentList = new Dictionary<IStateFunction, List<Segment>>();

        public List<Double> objectiveValues = new List<Double>();
        public List<Double> boundValues = new List<Double>();
        public List<Double> gapValues = new List<Double>();
        public Dictionary<String, Double> kpi2val = new Dictionary<String, Double>();

        public Dictionary<String, String> conflictingConstraints = new Dictionary<String, String>();
        public Dictionary<String, String> conflictingIntervalVars = new Dictionary<String, String>();
        public List<String> conflictingConstraintList = new List<String>();
        public List<String> conflictingIntervalVarsList = new List<String>();

        private Status solutionStatus = null;
        public bool hasConflicts = false;

        public WmlSolution()
        {
        }

        public WmlSolution(JObject solutionJson) : base()
        {
            Parse(solutionJson);
        }

        public void Reset()
        {
            name2val.Clear();
            var2val.Clear();

            name2start.Clear();
            var2start.Clear();

            name2size.Clear();
            var2size.Clear();

            name2end.Clear();
            var2end.Clear();

            objectiveValues.Clear();
            boundValues.Clear();
            gapValues.Clear();

            kpi2val.Clear();
            /*
             * objective = Double.NaN; status = -1; pfeas = false; dfeas = false;
             */
            conflictingConstraints.Clear();
            conflictingIntervalVars.Clear();
            conflictingConstraintList.Clear();
            conflictingIntervalVarsList.Clear();

            hasConflicts = false;
        }

        private Status GetStatus(String solveStatus)
        {
            switch (solveStatus)
            {
                case "Feasible":
                    return Status.Feasible;
                case "Optimal":
                    return Status.Optimal;
                case "Infeasible":
                    return Status.Infeasible;
                case "InfeasibleOrUnbounded":
                    return Status.InfeasibleOrUnbounded;
                case "Unbounded":
                    return Status.Unbounded;
                case "Unknown":
                    return Status.Unknown;
                default:
                    return Status.Error;
            }
        }

        private Status GetStatus(JObject solutionJson)
        {
            if (solutionJson.ContainsKey("solutionStatus"))
            {
                return GetStatus(
                        solutionJson.Value<JObject>("solutionStatus").Value<String>("solveStatus"));
            }
            return null;
        }

        public Status GetSolutionStatus()
        {
            return solutionStatus;
        }

        private double ParseDoubleFromString(String valueAsStr)
        {
            if (valueAsStr.ToLower().Equals("infinity"))
                return Double.PositiveInfinity;
            return double.Parse(valueAsStr);
        }

        private void Parse(JObject solutionJson)
        {
            Reset();

            solutionStatus = GetStatus(solutionJson);
            //ok
            if (solutionJson.ContainsKey("intVars"))
            {
                JObject intVars = solutionJson.Value<JObject>("intVars");
                IList<string> keys = intVars.Properties().Select(p => p.Name).ToList();

                foreach (var name in keys)
                {                    
                    Double value = intVars.Value<Double>(name);
                    name2val.Add(name, value);
                }
            }

            //ok
            if (solutionJson.ContainsKey("intervalVars"))
            {
                JObject intervalVars = solutionJson.Value<JObject>("intervalVars");
                IList<string> keys = intervalVars.Properties().Select(p => p.Name).ToList();

                foreach (var name in keys)
                {
                    JObject intervalVar = intervalVars.Value<JObject>(name);
                    if (intervalVar.ContainsKey("start"))
                    {
                        double start = intervalVar.Value<Double>("start");
                        name2start.Add(name, start);
                        double size = intervalVar.Value<Double>("size");
                        name2size.Add(name, size);
                        double end = intervalVar.Value<Double>("end");
                        name2end.Add(name, end);
                    }
                }
            }

            // ok?
            if (solutionJson.ContainsKey("sequenceVars"))
            {
                JObject intervalSequenceVars = solutionJson.Value<JObject>("sequenceVars");
                IList<string> keys = intervalSequenceVars.Properties().Select(p => p.Name).ToList();

                foreach (var name in keys)
                {
                    List<String> interval_seq_ids = new List<String>();
                    JArray intervalSequence = intervalSequenceVars.Value<JArray>(name);

                    foreach (var value in intervalSequence)
                    {
                        String interval_id = value.ToString();
                        interval_seq_ids.Add(interval_id);
                    }
                    name2nameList.Add(name, interval_seq_ids);
                }
            }

            //ok
            if (solutionJson.ContainsKey("stateFunctions"))
            {
                JObject stateFunctions = solutionJson.Value<JObject>("stateFunctions");
                IList<string> keys = stateFunctions.Properties().Select(p => p.Name).ToList();

                foreach (var name in keys)
                {
                    List<Segment> state_function_segments = new List<Segment>();
                    JArray stateFunction = stateFunctions.Value<JArray>(name);
                    foreach (var o in stateFunction)
                    {
                        JObject segment = (JObject)o;
                        state_function_segments.Add(new Segment(segment));
                    }
                    name2segmentList.Add((String)name, state_function_segments);
                }
            }

            //ok
            if (solutionJson.ContainsKey("KPIs"))
            {
                JObject kpIs = solutionJson.Value<JObject>("KPIs");
                IList<string> keys = kpIs.Properties().Select(p => p.Name).ToList();

                foreach (var name in keys)
                {
                    Double value = kpIs.Value<Double>(name);
                    kpi2val.Add((String)name, value);
                }
            }

            //ok
            if (solutionJson.ContainsKey("objectives"))
            {
                JArray objectives = solutionJson.Value<JArray>("objectives");
                foreach (var objValue in objectives)
                {
                    objectiveValues.Add(ParseDoubleFromString(objValue.ToString()));
                }
            }

            if (solutionJson.ContainsKey("bounds"))
            {
                JArray bounds = solutionJson.Value<JArray>("bounds");
                foreach (var boundValue in bounds)
                {
                    boundValues.Add(ParseDoubleFromString(boundValue.ToString()));
                }
            }

            if (solutionJson.ContainsKey("gaps"))
            {
                JArray gaps = solutionJson.Value<JArray>("gaps");
                foreach (var gapValue in gaps)
                {
                    gapValues.Add(ParseDoubleFromString(gapValue.ToString()));
                }
            }

            if (solutionJson.ContainsKey("conflict"))
            {
                JObject conflict = solutionJson.Value<JObject>("conflict");
                if (conflict.ContainsKey("constraints"))
                {
                    hasConflicts = true;
                    JObject constraints = conflict.Value<JObject>("constraints");
                    IList<string> keys = constraints.Properties().Select(p => p.Name).ToList();

                    foreach (var constraintId in keys)
                    {
                        String conflictStatus = constraints.Value<String>(constraintId);
                        conflictingConstraints.Add((String)constraintId, conflictStatus);
                    }
                }
                if (conflict.ContainsKey("intervalVars"))
                {
                    hasConflicts = true;
                    JObject intervalVars = conflict.Value<JObject>("intervalVars");
                    IList<string> keys = intervalVars.Properties().Select(p => p.Name).ToList();

                    foreach (var intervalVarId in keys)
                    {
                        String conflictStatus = intervalVars.Value<String>(intervalVarId);
                        conflictingIntervalVars.Add((String)intervalVarId, conflictStatus);
                    }
                }
            }
        }
    }

}
