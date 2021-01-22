using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ILOG.Concert;


namespace ILOG.CPLEX
{
    /**
     * Solution information.
     * Instances of this class contain solution information that can be obtained from a CPLEX <code>.sol</code> file.
     *
     */
    public class Solution
    {
        private enum ParserState
        {
            INITIAL,
            SOLUTION,
            HEADER,
            QUALITY,
            VARIABLES,
            LINEAR_CONSTRAINTS,
            UNKNOWN,
            FINISHED
        }

        private String solution = null;

        public bool feasible = false;
        /**
         * Map variable names to values.
         */
        public Dictionary<String, Double> name2val = new Dictionary<String, Double>();
        public Dictionary<String, Double> name2ReducedCost = new Dictionary<String, Double>();
        /**
         * Map variable objects to values.
         */
        public Dictionary<INumVar, Double> var2val = new Dictionary<INumVar, Double>();
        public Dictionary<INumVar, Double> var2ReducedCost = new Dictionary<INumVar, Double>();
        /**
         * Map range names to values.
         */
        public Dictionary<String, Double> name2dual = new Dictionary<String, Double>();
        /**
         * Map range objects to values.
         */
        public Dictionary<IRange, Double> rng2dual = new Dictionary<IRange, Double>();
        /**
         * Map range names to values.
         */
        public Dictionary<String, Double> name2slack = new Dictionary<String, Double>();
        /**
         * Map range objects to values.
         */
        public Dictionary<IRange, Double> rng2slack = new Dictionary<IRange, Double>();
        /**
         * Objective value of solution.
         */
        public double objective = Double.NaN;
        /**
         * CPLEX status.
         */
        public int status = -1;
        /**
         * Primal feasible?
         */
        public bool pfeas = false;
        /**
         * Dual feasible?
         */
        public bool dfeas = false;

        public Solution(int status)
        {
            this.status = status;
        }

        public Solution(String solutionXml, HashSet<String> knownVariables, HashSet<String> knownConstraints)
        {
            this.status = -1;
            Parse(solutionXml, knownVariables, knownConstraints);
        }

        public bool HasSolution()
        {
            return solution != null;
        }

        public String GetSolution()
        {
            return solution;
        }

        public void Reset()
        {
            solution = null;
            feasible = false;
            name2val.Clear();
            //name2val = new Dictionary<String, Double>();
            var2val.Clear();
            //var2val = new Dictionary<INumVar, Double>();
            name2ReducedCost.Clear();
            //name2ReducedCost = new Dictionary<String, Double>();
            name2dual.Clear();
            //name2dual = new Dictionary<String, Double>();
            rng2dual.Clear();
            //rng2dual = new Dictionary<IRange, Double>();
            name2slack.Clear();
            //name2slack = new Dictionary<String, Double>();\
            rng2slack.Clear();
            //rng2slack = new Dictionary<IRange, Double>();
            objective = Double.NaN;
            status = -1;
            pfeas = false;
            dfeas = false;
        }

        /**
         * Parse a CPLEX <code>.sol</code> file.
         * See {@link #parse(InAddStream, Set, Set)} for details.
         */
        public void Parse(String solutionXml, HashSet<String> knownVariables, HashSet<String> knownConstraints)
        {
            Reset();
            Stream inAdd = new MemoryStream(Encoding.UTF8.GetBytes(solutionXml));
            Parse(inAdd, knownVariables, knownConstraints);
        }

        private static String[] GetAttributes(XmlReader reader, String attr)
        {
            return GetAttributes(reader, new string[] { attr });
        }
        private static String[] GetAttributes(XmlReader reader, String attr1, String attr2)
        {
            return GetAttributes(reader, new string[] { attr1, attr2 });
        }
        private static String[] GetAttributes(XmlReader reader, String attr1, String attr2, String attr3)
        {
            return GetAttributes(reader, new string[] { attr1, attr2, attr3 });
        }
        private static String[] GetAttributes(XmlReader reader, String attr1, String attr2, String attr3, String attr4)
        {
            return GetAttributes(reader, new string[] { attr1, attr2, attr3, attr4 });
        }

        private static String[] GetAttributes(XmlReader reader, String[] attrs)
        {
            String[] ret = new String[attrs.Length];
            Dictionary<String, String> attrMap = new Dictionary<String, String>();
            foreach (String s in attrs)
                attrMap.Add(s, null);

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if (attrMap.ContainsKey(reader.Name))
                        attrMap[reader.Name] =reader.Value;
                }
            }

            for (int i = 0; i < attrs.Length; ++i) {
                String v;
                attrMap.TryGetValue(attrs[i], out v);
                ret[i] = v;
            }
            return ret;
        }

        /**
         * Parse a CPLEX <code>.sol</code> file.
         *
         * @param solutionXml    The CPLEX <code>.sol</code> file to parse.
         * @param knownVariables The names of the variables for which values should be extracted from <code>solutionXml</code>.
         * @throws IOException If an inAdd/outAdd error occurs or mandatory solution information is missing.
         */
        private void Parse(Stream inAdd, HashSet<String> knownVariables, HashSet<String> knownConstraints)
        {
            Reset();
            XmlReaderSettings settings = new XmlReaderSettings();
            String MALFORMED_XML = "Malformed XML";

            using (XmlReader reader = XmlReader.Create(inAdd, settings))
            {
                IList<String> unknownStack = new List<String>();// new String[4]; /* Parsing anything but variables, header, quality in a CPLEXSolution element. */
                ParserState state = ParserState.INITIAL;
                int solnum = -1; // Solution number for error messages.
                Boolean ok = false;
                while (reader.Read())
                {
                    String element;
                    String[] attrs;
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            element = reader.Name;
                            switch (state)
                            {
                                case ParserState.INITIAL:
                                    // First element must be <CPLEXSolution>
                                    if (element.Equals("CPLEXSolution"))
                                    {
                                        ++solnum;
                                        state = ParserState.SOLUTION;
                                        this.feasible = true;
                                    }
                                    else
                                        throw new IOException(MALFORMED_XML);
                                    break;
                                case ParserState.HEADER:
                                    throw new IOException(MALFORMED_XML);
                                case ParserState.QUALITY:
                                    throw new IOException(MALFORMED_XML);
                                case ParserState.SOLUTION:
                                    if (element.Equals("header"))
                                    {
                                        state = ParserState.HEADER;
                                        attrs = GetAttributes(reader, "objectiveValue", "solutionStatusValue", "primalFeasible", "dualFeasible");
                                        if (attrs[0] == null)
                                            throw new IOException("No objective for solution " + solnum);
                                        this.objective = Double.Parse(attrs[0]);
                                        if (attrs[1] == null)
                                            throw new IOException("No solution status for solution " + solnum);
                                        this.status = Int32.Parse(attrs[1]);
                                        this.pfeas = attrs[2] != null && Int32.Parse(attrs[2]) != 0;
                                        this.dfeas = attrs[3] != null && Int32.Parse(attrs[3]) != 0;
                                    }
                                    else if (element.Equals("quality"))
                                    {
                                        state = ParserState.QUALITY;
                                    }
                                    else if (element.Equals("variables"))
                                    {
                                        state = ParserState.VARIABLES;
                                    }
                                    else if (element.Equals("linearConstraints"))
                                    {
                                        state = ParserState.LINEAR_CONSTRAINTS;
                                    }
                                    else if (element.Equals("indicatorConstraints"))
                                    {
                                        state = ParserState.LINEAR_CONSTRAINTS;
                                    }
                                    else
                                    {
                                        state = ParserState.UNKNOWN;
                                        unknownStack.Add(element);
                                    }
                                    break;
                                case ParserState.VARIABLES:
                                    if (!element.Equals("variable"))
                                        throw new IOException(MALFORMED_XML);
                                    attrs = GetAttributes(reader, "name", "value", "reducedCost");
                                    if (attrs[0] != null)
                                    {
                                        if (attrs[1] == null)
                                            throw new IOException("Variable without value in solution file for solution " + solnum);
                                        if (knownVariables.Contains(attrs[0]))
                                            this.name2val.Add(attrs[0], Double.Parse(attrs[1]));
                                        if (attrs[2] == null)
                                        {
                                            // CAN BE MIP
                                            //ignore it
                                        }
                                        else
                                        {
                                            if (knownVariables.Contains(attrs[0]))
                                                this.name2ReducedCost.Add(attrs[0], Double.Parse(attrs[2]));
                                        }
                                    }
                                    break;
                                case ParserState.LINEAR_CONSTRAINTS:
                                    attrs = GetAttributes(reader, "name", "dual", "slack");
                                    if (attrs[0] != null)
                                    {
                                        if (attrs[1] == null)
                                        {
                                            // CAN BE MIP
                                            //ignore it
                                        }
                                        else
                                        {
                                            if (knownConstraints.Contains(attrs[0]))
                                                this.name2dual.Add(attrs[0], Double.Parse(attrs[1]));
                                        }
                                        if (attrs[2] == null)
                                        {
                                            // CAN BE MIP
                                            //ignore it
                                        }
                                        else
                                        {
                                            if (knownConstraints.Contains(attrs[0]))
                                                this.name2slack.Add(attrs[0], Double.Parse(attrs[2]));
                                        }
                                    }
                                    break;
                                case ParserState.UNKNOWN:
                                    unknownStack.Add(element);
                                    break;
                                case ParserState.FINISHED:
                                    // this cannot happen
                                    throw new IOException(MALFORMED_XML);
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.EndElement:
                            element = reader.Name;
                            switch (state)
                            {
                                case ParserState.INITIAL:
                                    // This should not happen since we stop after the first solution
                                    throw new IOException(MALFORMED_XML);
                                case ParserState.SOLUTION:
                                    if (!element.Equals("CPLEXSolution"))
                                        throw new IOException(MALFORMED_XML);
                                    // We only parse the very first solution in the file.
                                    state = ParserState.FINISHED;
                                    break;
                                case ParserState.HEADER:
                                    if (!element.Equals("header"))
                                        throw new IOException(MALFORMED_XML);
                                    state = ParserState.SOLUTION;
                                    break;
                                case ParserState.QUALITY:
                                    if (!element.Equals("quality"))
                                        throw new IOException(MALFORMED_XML);
                                    state = ParserState.SOLUTION;
                                    break;
                                case ParserState.VARIABLES:
                                    if (element.Equals("variable")) { /* nothing */ }
                                    else if (element.Equals("variables"))
                                    {
                                        state = ParserState.SOLUTION;
                                    }
                                    else
                                        throw new IOException(MALFORMED_XML);
                                    break;
                                case ParserState.LINEAR_CONSTRAINTS:
                                    if (element.Equals("constraint")) { /* nothing */ }
                                    else if (element.Equals("linearConstraints"))
                                    {
                                        state = ParserState.SOLUTION;
                                    }
                                    else if (element.Equals("indicatorConstraints"))
                                    {
                                        state = ParserState.SOLUTION;
                                    }
                                    else
                                        throw new IOException(MALFORMED_XML);
                                    break;
                                case ParserState.UNKNOWN:
                                    if (unknownStack.Count == 0 || !element.Equals(unknownStack.Last()))
                                        throw new IOException(MALFORMED_XML);
                                    unknownStack.RemoveAt(unknownStack.Count - 1);
                                    if (unknownStack.Count == 0)
                                        state = ParserState.SOLUTION;
                                    break;
                                case ParserState.FINISHED:
                                    // This cannot happen
                                    throw new IOException(MALFORMED_XML);
                            }
                            break;                            
                        default:
                            break;
                    }
                }
            }
        }

    }

}