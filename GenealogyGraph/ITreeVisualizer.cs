using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenealogyGraph
{
    public interface ITreeVisualizer<TInput, TTree>
    {
        /// <summary>
        /// Creates a visualisation for Genealogy Tree
        /// </summary>
        /// <param name="input"></param>
        public TTree VisualizeTree(TInput input);
    }
}
