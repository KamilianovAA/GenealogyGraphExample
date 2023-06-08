using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenealogyGraph
{
    public interface ITreeWriter<T>
    {
        /// <summary>
        /// Writes genealogical tree to a file specified by path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tree"></param>
        public void WriteTreeToFile(string path, T tree);
    }
}
