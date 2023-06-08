namespace GenealogyGraph
{
    public class Person
    {
        /// <summary>
        /// Default value is -1
        /// </summary>
        public int Id { get; set; }
        public Name? Name { get; set; }
        public Sex Sex { get; set; }
        public List<Family>? Families { get; set; }
        public List<Family>? ParentFamilies { get; set; }

        public Sex Sex1
        {
            get => default;
            set
            {
            }
        }

        public Name Name1
        {
            get => default;
            set
            {
            }
        }

        public Person(Name name, int id = -1)
        {
            Name = name;
            Id = id;
        }
    }
}
