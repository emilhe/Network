﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public interface IVectorSolution<T> : ISolution where T : IVectorSolution<T>
    {

        T Add(double[] vec, double weight);
        double[] Delta(T other);

        double[] GetVectorCopy();

    }
}
