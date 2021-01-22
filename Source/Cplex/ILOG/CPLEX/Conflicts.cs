using ILOG.Concert;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILOG.CPLEX
{
    public class Conflicts
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Conflicts));

        private enum Type { LB, UB, LIN, QUAD, IND, SOS }

        public String getName(IConstraint obj)
        {
            if (obj.Name == null)
            {
                logger.Error("Missing name for relaxing " + obj);
                throw new Concert.Exception("Missing name for relaxing " + obj);
            }
            return obj.Name;
        }

        private Type GetType(IConstraint obj)
        {
            if (obj is ISOS1 || obj is ISOS2) return Type.SOS;
            if (obj is CpxIfThen) return Type.IND;
            if (obj is IRange)
            {
                IRange range = (IRange)obj;
                INumExpr expr = range.Expr;
                if (expr is CpxQLExpr)
                {
                    //TODO
                    throw new Concert.Exception("This is not supported yet");
                    //CpxQLExpr x = (CpxQLExpr)expr;
                    //if (x._quad != null)
                   //{//
                     //   if (x.getQuadVar1() != null)
                       //     return Type.QUAD;
                        //else return Type.LIN;
                    //}
                    //return Type.LIN;
                }
                return Type.LIN;
            }
            if (obj is CpxAnd || obj is CpxOr || obj is CpxNot)
            {
                logger.Error("Type of constraint is not supported by WML conflict refiner: " + obj);
                throw new Concert.Exception("Type of constraint is not supported by WML conflict refiner: " + obj);
            }
            return Type.LIN;
        }

        private Dictionary<Double, List<IConstraint>> elems = new Dictionary<Double, List<IConstraint>>();

        public Conflicts()
        {
        }

        public int GetSize()
        {
            int size = 0;
            foreach (var elem in elems)
            {
                size = size + elem.Value.Count;
            }
            return size;
        }

        public void Add(IConstraint o, double p)
        {
            if (o is CpxAnd || o is CpxOr || o is CpxNot)
            {
                logger.Error("Type of constraint is not supported by WML conflict refiner: " + o);
                throw new Concert.Exception("Type of constraint is not supported by WML conflict refiner: " + o);
            }
            if (!elems.ContainsKey(p))
                elems.Add(p, new List<IConstraint>());

            List<IConstraint> list;
            elems.TryGetValue(p, out list);
            list.Add(o);
        }

        private void FillBuffer(StringBuilder buffer, List<IConstraint> conflict, double preference)
        {
            buffer.Append("<group preference='").Append(preference).Append("'>");
            foreach (IConstraint e in conflict)
                buffer.Append("<con").Append(" name='").Append(getName(e)).Append("'").Append(" type='").Append(GetType(e).ToString().ToLower()).Append("'/>");
            buffer.Append("</group>");
        }

        public byte[] MakeFile()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("<CPLEXRefineconflictext resultNames='true'>");
            foreach (var elem in elems)
            {
                FillBuffer(buffer, elem.Value, elem.Key);
            }
            buffer.Append("</CPLEXRefineconflictext>");
            String c = buffer.ToString();
            logger.Info("Conflict file is " + c);
            return Encoding.UTF8.GetBytes(c);
        }
    }
}
