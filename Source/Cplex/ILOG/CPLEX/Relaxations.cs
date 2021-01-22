using ILOG.Concert;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILOG.CPLEX
{
    public class Relaxations
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Relaxations));

        private class Relax
        {
            public class Elem
            {
                IAddable obj;
                public double preference;

                public Elem(IAddable obj, double p)
                {
                    this.obj = obj;
                    this.preference = p;
                }

                public String GetName()
                {
                    if (this.obj.Name == null)
                    {
                        logger.Error("Missing name for relaxing " + obj);
                        throw new Concert.Exception("Missing name for relaxing " + obj);
                    }
                    return obj.Name;
                }
            }

            public int GetSize()
            {
                return elems.Count;
            }

            private HashSet<Elem> elems = new HashSet<Elem>();


            public Relax()
            {
            }

            public Relax Add(IAddable obj, double preference)
            {
                elems.Add(new Elem(obj, preference));
                return this;
            }

            private static void FillBuffer(StringBuilder buffer, Relax relax, String tag)
            {
                String relaxName = "<relax name='";
                String preference = "' preference='";
                String end = "'/>";
                buffer.Append("<").Append(tag).Append(">");
                foreach (Elem e in relax.elems)
                    buffer.Append(relaxName).Append(e.GetName()).Append(preference).Append(e.preference).Append(end);
                buffer.Append("</").Append(tag).Append(">");
            }

            public static byte[] MakeFile(Relax rhs, Relax rng, Relax lb, Relax ub)
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append("<CPLEXFeasopt infeasibilityFile='true' resultNames='true'>");
                if (rhs.GetSize() != 0) FillBuffer(buffer, rhs, "rhs");
                if (rng.GetSize() != 0) FillBuffer(buffer, rng, "rng");
                if (lb.GetSize() != 0) FillBuffer(buffer, lb, "lb");
                if (ub.GetSize() != 0) FillBuffer(buffer, ub, "ub");
                buffer.Append("</CPLEXFeasopt>");
                String r = buffer.ToString();
                logger.Info("Relaxations file is " + r);
                return Encoding.UTF8.GetBytes(r);
            }
        }

        private Relax rhs = new Relax();
        private Relax rng = new Relax();
        private Relax lb = new Relax();
        private Relax ub = new Relax();

        public Relaxations()
        {
        }

        public int GetSize()
        {
            return rhs.GetSize() + rng.GetSize() + lb.GetSize() + ub.GetSize();
        }

        public void Add(IRange r, double p)
        {
            rng.Add(r, p);
        }

        public void Add(IConstraint r, double p)
        {
            if (r is IRange)
            {
                IRange range = (IRange)r;
                if (range.LB == -Double.MaxValue || range.UB == Double.MaxValue)
                {
                    rhs.Add(r, p);
                }
                else rng.Add(r, p);
            }
            else
                throw new Concert.Exception("Only ranges are supported in WML feasopt.");
        }

        private void _add(IAddable a, double lbound, double ubound)
        {
            lb.Add(a, lbound);
            ub.Add(a, ubound);
        }

        public void Add(IRange r, double lbound, double ubound)
        {
            _add(r, lbound, ubound);
        }

        public void Add(INumVar v, double lbound, double ubound)
        {
            _add(v, lbound, ubound);
        }

        public byte[] MakeFile()
        {
            return Relax.MakeFile(rhs, rng, lb, ub);
        }
    }

}
