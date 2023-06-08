namespace GenealogyGraph
{
    public interface ITreeReader<T>
    {
        /// <summary>
        /// Reads genealogy tree from file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<T> ReadTreeFromFile(string path);

        /// <summary>
        /// Returns genealogy tree for a specific person
        /// </summary>
        /// <param name="path"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? GetPersonRelations(string path, int id, int depth);
    }
}
